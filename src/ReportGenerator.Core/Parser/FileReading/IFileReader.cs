namespace Palmmedia.ReportGenerator.Core.Parser.FileReading
{
    /// <summary>
    /// Interface for file access.
    /// </summary>
    internal interface IFileReader
    {
        /// <summary>
        /// Loads the file with the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="error">Error message if file reading failed, otherwise <code>null</code>.</param>
        /// <returns><code>null</code> if an error occurs, otherwise the lines of the file.</returns>
        string[] LoadFile(string path, out string error);
    }
}
