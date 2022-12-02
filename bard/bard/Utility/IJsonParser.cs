namespace bard.Utility
{
    internal interface IJsonParser
    {
        /// <summary>
        /// Opens the file with given <see cref="fileInfo"/> and parses its text content based on the type <see cref="T"/>
        /// </summary>
        Task<T> ReadAsync<T>(FileInfo fileInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses the <see cref="text"/> content based on the type <see cref="T"/>
        /// </summary>
        Task<T> ReadAsync<T>(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Write JSON <see cref="data"/> to given file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data to write</param>
        /// <param name="fileInfo">The file to edit</param>
        /// <param name="cancellationToken">The cancellation token to interrupt the operation</param>
        /// <returns></returns>
        Task WriteAsync<T>(T data, FileInfo fileInfo, CancellationToken cancellationToken = default);
    }
}
