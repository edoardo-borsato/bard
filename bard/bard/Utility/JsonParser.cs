using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace bard.Utility;

internal class JsonParser : IJsonParser
{
    private readonly ILogger _logger;

    public JsonParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<JsonParser>();
    }

    public async Task<T> ReadAsync<T>(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Reading JSON content from file. File name {0}", fileInfo.FullName);

        var text = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken);
        return await ReadAsync<T>(text, cancellationToken);
    }

    public async Task<T> ReadAsync<T>(string text, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Parsing JSON content");
        var stream = await text.ToStreamAsync();
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var deserializedObject = await JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions, cancellationToken);
        _logger.LogTrace("JSON content parsed");

        return deserializedObject!;
    }

    public async Task WriteAsync<T>(T data, FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        using (var stream = new MemoryStream())
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions, cancellationToken);
            await File.WriteAllBytesAsync(fileInfo.FullName, stream.ToArray(), cancellationToken);

            _logger.LogDebug("JSON content wrote to file. File name {0}", fileInfo.FullName);
        }
    }
}