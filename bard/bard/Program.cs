﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using bard.CommandLine;
using bard.Utility;
using CommandLine;
using Microsoft.Extensions.Logging;
#pragma warning disable CS8618

namespace bard
{
    internal static class Program
    {
        private static IJsonParser _jsonParser;
        private static ILogger _logger;

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

        private static Task DoRestoreAsync(RestoreOption option)
        {
            throw new NotImplementedException();
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

                Environment.SetEnvironmentVariable("AWS_PROFILE", option.AwsProfile);
                Environment.SetEnvironmentVariable("AWS_REGION", option.AwsRegion);

                var client = new AmazonDynamoDBClient();
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
            finally
            {
                Environment.SetEnvironmentVariable("AWS_PROFILE", null);
                Environment.SetEnvironmentVariable("AWS_REGION", null);
            }
        }

        #endregion
    }
}