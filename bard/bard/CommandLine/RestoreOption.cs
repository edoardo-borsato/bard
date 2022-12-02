using CommandLine;

namespace bard.CommandLine;

[Verb("restore", HelpText = "Restore data from given JSON file to a Dynamo DB table")]
internal record RestoreOption
{
    [Option('t', "table", Required = true, HelpText = "The name of the table")]
    public string Table { get; init; }

    [Option('f', "file", Required = true, HelpText = "The backup JSON file full path")]
    public string FilePath { get; init; }

    [Option('a', "aws-profile", Required = true, HelpText = "The AWS profile to use")]
    public string AwsProfile { get; init; }

    [Option('r', "aws-region", Required = false, HelpText = "The AWS region containing the table. If not provided it will be used the AWS profile default region")]
    public string AwsRegion { get; init; }
}