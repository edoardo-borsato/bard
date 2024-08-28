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
        var result = parser.ParseArguments<BackUpOption, RestoreOption>(args);
        result
            .WithParsed<BackUpOption>(option => DoBackupAsync(option).Wait())
            .WithParsed<RestoreOption>(option => DoRestoreAsync(option).Wait());
    }

    #region Utility Methods

    private static async Task DoRestoreAsync(RestoreOption option)
    {
        try
        {
            _logger.LogInformation("Starting restore: {0} {{{1}}}, {2} {{{3}}}, {4} {{{5}}}, {6} {{{7}}}",
                nameof(option.Table), option.Table,
                nameof(option.FilePath), option.FilePath,
                nameof(option.AwsProfile), option.AwsProfile,
                nameof(option.AwsRegion), option.AwsRegion);

            var jsonItems = await _jsonParser.ReadAsync<IEnumerable<IDictionary<string, object>>>(new FileInfo(option.FilePath));
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
            _logger.LogInformation("Starting backup: {0} {{{1}}}, {2} {{{3}}}, {4} {{{5}}}, {6} {{{7}}}",
                nameof(option.Table), option.Table,
                nameof(option.FilePath), option.FilePath,
                nameof(option.AwsProfile), option.AwsProfile,
                nameof(option.AwsRegion), option.AwsRegion);

            new CredentialProfileStoreChain().TryGetAWSCredentials(option.AwsProfile, out var awsCredentials);
            var client = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.GetBySystemName(option.AwsRegion));
            var request = new ScanRequest(option.Table);
            var items = (await client.ScanAsync(request)).Items;

            var jsonItems = items.ToJsonItems();

            await _jsonParser.WriteAsync(jsonItems, new FileInfo(option.FilePath));

            _logger.LogInformation("Backup completed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred during backup");
        }
    }

    #endregion
}