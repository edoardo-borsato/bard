using CommandLine;

namespace bard.CommandLine;

[Verb("restore", HelpText = "Restores data from given JSON file to a Dynamo DB table")]
internal record RestoreOption
{
    [Option('t', "table", Required = true, HelpText = "The name of the table")]
    public required string Table { get; init; }

    [Option('f', "file", Required = true, HelpText = "The backup JSON file full path")]
    public required string FilePath { get; init; }

    [Option('p', "aws-profile", Required = true, HelpText = "The AWS profile to use")]
    public required string AwsProfile { get; init; }

    [Option('r', "aws-region", Required = false, HelpText = "The AWS region containing the table. If not provided it will be used the AWS profile default region")]
    public string? AwsRegion { get; init; }
}