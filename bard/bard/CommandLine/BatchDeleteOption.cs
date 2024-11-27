using CommandLine;

namespace bard.CommandLine;

[Verb("batch-delete", HelpText = "Deletes all records matching given partition key and/or sort key from a Dynamo DB table")]
internal record BatchDeleteOption
{
    [Option('t', "table", Required = true, HelpText = "The name of the table")]
    public required string Table { get; init; }

    [Option("partition-key-name", Required = true, HelpText = "The partition key name")]
    public required string PartitionKeyName { get; init; }

    [Option("partition-key-type", Required = true, HelpText = "The partition key type. Supported types are `B` for binary, `S` for string and `N` for number")]
    public required string PartitionKeyType { get; init; }

    [Option("partition-key-value", Required = true, HelpText = "The partition key value")]
    public required string PartitionKeyValue { get; init; }

    [Option("sort-key-name", Required = false, HelpText = "The sort key name")]
    public string? SortKeyName { get; init; }

    [Option("sort-key-type", Required = false, HelpText = "The sort key type. Supported types are `B` for binary, `S` for string and `N` for number")]
    public string? SortKeyType { get; init; }

    [Option("sort-key-value", Required = false, HelpText = "The sort key value")]
    public string? SortKeyValue { get; init; }

    [Option('p', "aws-profile", Required = true, HelpText = "The AWS profile to use")]
    public required string AwsProfile { get; init; }

    [Option('r', "aws-region", Required = false, HelpText = "The AWS region containing the table. If not provided it will be used the AWS profile default region")]
    public string? AwsRegion { get; init; }
}