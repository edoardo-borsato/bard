using System.Diagnostics;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.CredentialManagement;
using bard.CommandLine;
using bard.Utility;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace bard;

internal static class Program
{
    private static IJsonParser _jsonParser = null!;
    private static ILogger _logger = null!;

    public static void Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Trace));

        _jsonParser = new JsonParser(loggerFactory);
        _logger = loggerFactory.CreateLogger("bard");

        var parser = Parser.Default;
        var result = parser.ParseArguments<BackUpOption, RestoreOption, BatchDeleteOption>(args);
        result
            .WithParsed<BackUpOption>(option => DoBackupAsync(option).Wait())
            .WithParsed<RestoreOption>(option => DoRestoreAsync(option).Wait())
            .WithParsed<BatchDeleteOption>(option => DoBatchDeleteAsync(option).Wait());
    }

    #region Utility Methods

    private static async Task DoRestoreAsync(RestoreOption option)
    {
        try
        {
            _logger.LogInformation("Starting restore: {table} {{{tableName}}}, {filePath} {{{filePathValue}}}, {profile} {{{profileName}}}, {region} {{{regionValue}}}",
                nameof(option.Table), option.Table,
                nameof(option.FilePath), option.FilePath,
                nameof(option.AwsProfile), option.AwsProfile,
                nameof(option.AwsRegion), option.AwsRegion);

            var jsonItems = (await _jsonParser.ReadAsync<IEnumerable<IDictionary<string, object>>>(new FileInfo(option.FilePath))).ToList();

            _logger.LogDebug("Items retrieved. Count: {count}", jsonItems.Count);

            var awsItems = jsonItems.ToAwsItems();
            new CredentialProfileStoreChain().TryGetAWSCredentials(option.AwsProfile, out var awsCredentials);
            var client = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.GetBySystemName(option.AwsRegion));
            var request = CreateBatchWriteItemRequest(option.Table, awsItems);

            await client.BatchWriteItemAsync(request);

            _logger.LogInformation("Restore completed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred during restore");
        }
    }

    private static BatchWriteItemRequest CreateBatchWriteItemRequest(string table, IEnumerable<IDictionary<string, AttributeValue?>> awsItems)
    {
        var writeRequests = awsItems.Select(item =>
            new WriteRequest(new PutRequest
            {
                Item = new Dictionary<string, AttributeValue?>(item)
            })).ToList();

        var request = new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                { table, writeRequests }
            }
        };
        return request;
    }

    private static async Task DoBackupAsync(BackUpOption option)
    {
        try
        {
            _logger.LogInformation("Starting backup: {table} {{{tableName}}}, {filePath} {{{filePathValue}}}, {profile} {{{profileName}}}, {region} {{{regionValue}}}",
                nameof(option.Table), option.Table,
                nameof(option.FilePath), option.FilePath,
                nameof(option.AwsProfile), option.AwsProfile,
                nameof(option.AwsRegion), option.AwsRegion);
            var sw = Stopwatch.StartNew();

            new CredentialProfileStoreChain().TryGetAWSCredentials(option.AwsProfile, out var awsCredentials);
            var client = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.GetBySystemName(option.AwsRegion));

            var items = new List<Dictionary<string, AttributeValue>>();
            Dictionary<string, AttributeValue>? lastKeyEvaluated = null;
            
            do
            {
                var request = new ScanRequest(option.Table)
                {
                    ExclusiveStartKey = lastKeyEvaluated
                };
                var response = await client.ScanAsync(request);
                items.AddRange(response.Items);
                lastKeyEvaluated = response.LastEvaluatedKey;
                _logger.LogDebug("Items retrieved in this page. Count: {count}", items.Count);
            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            _logger.LogDebug("Items retrieved. Count: {count}. Elapsed {elapsed}", items.Count, sw.Elapsed);

            var jsonItems = items.ToJsonItems();

            await _jsonParser.WriteAsync(jsonItems, new FileInfo(option.FilePath));

            _logger.LogDebug("Items wrote to JSON file");

            _logger.LogInformation("Backup completed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred during backup");
        }
    }

    private static async Task DoBatchDeleteAsync(BatchDeleteOption option)
    {
        try
        {
            _logger.LogInformation("Starting batch-delete: {table} {{{tableName}}}, {pkName} {{{pkNameValue}}}, {pkType} {{{pkTypeValue}}}, {pkValue} {{{pkValueValue}}}, {skName} {{{skNameValue}}}, {skType} {{{skTypeValue}}}, {skValue} {{{skValueValue}}}, {profile} {{{profileName}}}, {region} {{{regionValue}}}",
                nameof(option.Table), option.Table,
                nameof(option.PartitionKeyName), option.PartitionKeyName,
                nameof(option.PartitionKeyType), option.PartitionKeyType,
                nameof(option.PartitionKeyValue), option.PartitionKeyValue,
                nameof(option.SortKeyName), option.SortKeyName,
                nameof(option.SortKeyType), option.SortKeyType,
                nameof(option.SortKeyValue), option.SortKeyValue,
                nameof(option.AwsProfile), option.AwsProfile,
                nameof(option.AwsRegion), option.AwsRegion);

            new CredentialProfileStoreChain().TryGetAWSCredentials(option.AwsProfile, out var awsCredentials);
            var client = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.GetBySystemName(option.AwsRegion));

            var keyConditionExpression = "#pk = :pk";
            var expressionAttributeNames = new Dictionary<string, string>
            {
                { "#pk", option.PartitionKeyName }
            };
            var expressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", GetAttributeValue(option.PartitionKeyType, option.PartitionKeyValue) }
            };

            if (option.SortKeyName is not null)
            {
                if (option.SortKeyType is null || option.SortKeyValue is null)
                {
                    _logger.LogError("When sort-key-name is set, all sort-key related parameter must be provide");
                    return;
                }

                keyConditionExpression += " AND #sk = :sk";
                expressionAttributeNames.Add("#sk", option.SortKeyName);
                expressionAttributeValues.Add(":sk", GetAttributeValue(option.SortKeyType, option.SortKeyValue));
            }

            // --key-condition-expression "OrganizationId = :pk" --expression-attribute-values '{\":pk\":{\"S\":\"aba45536-a938-4464-aad6-5f56c31a6a56\"}}'"
            var queryRequest = new QueryRequest(option.Table)
            {
                KeyConditionExpression = keyConditionExpression,
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues
            };
            var items = (await client.QueryAsync(queryRequest)).Items;

            _logger.LogDebug("Items retrieved. Count: {count}", items.Count);

            var deleteTasks = new List<Task>();
            var count = 0;
            foreach (var item in items)
            {
                _logger.LogTrace("Deleting item: {currentItem} of {itemsCount}", ++count, items.Count);
                deleteTasks.Add(client.DeleteItemAsync(new DeleteItemRequest(option.Table, item)));
                await Task.Delay(TimeSpan.FromMilliseconds(20));    // add some delay to not use too much AWS write capacity
            }

            _logger.LogDebug("Items deleted");

            Task.WaitAll(deleteTasks.ToArray());

            _logger.LogInformation("Batch-delete completed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred during Batch-delete");
        }
    }

    private static AttributeValue GetAttributeValue(string type, string value)
    {
        return type switch
        {
            "B" => new AttributeValue { B = value.ToStream() },
            "S" => new AttributeValue { S = value },
            "N" => new AttributeValue { N = value },
            _ => throw new ArgumentOutOfRangeException($"Invalid key type: {type}. Supported types are `B`, `S` and `N`")
        };
    }

    #endregion
}