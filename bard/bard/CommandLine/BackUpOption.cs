using CommandLine;

namespace bard.CommandLine;

[Verb("backup", HelpText = "Backup given Dynamo DB table to a JSON file")]
internal record BackUpOption
{
    [Option('t', "table", Required = true, HelpText = "The name of the table")]
    public required string Table { get; init; }

    [Option('f', "file", Required = true, HelpText = "The backup JSON file full path")]
    public required string FilePath { get; init; }

    [Option('p', "aws-profile", Required = true, HelpText = "The AWS profile to use")]
    public required string AwsProfile { get; init; }

    [Option('r', "aws-region", Required = false, HelpText = "The AWS region containing the table. If not provided it will be used the AWS profile default region")]
    public string? AwsRegion { get; init; }

    [Option('m', "method", Required = true, HelpText = "The AWS Dynamo DB method to use. Possible values are `scan` or `query`. In case of `query` partition key and sort key info are required")]
    public string? Method { get; init; }

    [Option("partition-key-name", Required = false, HelpText = "The partition key name")]
    public string? PartitionKeyName { get; init; }

    [Option("partition-key-type", Required = false, HelpText = "The partition key type. Supported types are `B` for binary, `S` for string and `N` for number")]
    public string? PartitionKeyType { get; init; }

    [Option("partition-key-value", Required = false, HelpText = "The partition key value")]
    public string? PartitionKeyValue { get; init; }

    [Option("sort-key-name", Required = false, HelpText = "The sort key name")]
    public string? SortKeyName { get; init; }

    [Option("sort-key-type", Required = false, HelpText = "The sort key type. Supported types are `B` for binary, `S` for string and `N` for number")]
    public string? SortKeyType { get; init; }

    [Option("sort-key-value", Required = false, HelpText = "The sort key value")]
    public string? SortKeyValue { get; init; }
}