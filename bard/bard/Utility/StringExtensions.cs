namespace bard.Utility
{
    internal static class StringExtensions
    {
        public static async Task<Stream> ToStreamAsync(this string str)
        {
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync(str);
                    await streamWriter.FlushAsync();
                    stream.Position = 0;

                    return stream;
                }
            }
        }
    }
}
