using System.Collections.Generic;
using System.IO;

namespace Palmmedia.ReportGenerator.Core.Reporting.History
{
    /// <summary>
    /// Interface for storage of history reports.
    /// </summary>
    public interface IHistoryStorage
    {
        /// <summary>
        /// Gets the history file paths.
        /// </summary>
        /// <returns>The history file paths.</returns>
        IEnumerable<string> GetHistoryFilePaths();

        /// <summary>
        /// Loads the given file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The file as stream.</returns>
        Stream LoadFile(string filePath);

        /// <summary>
        /// Saves the file with the given name.
        /// </summary>
        /// <param name="stream">The stream containing the file content.</param>
        /// <param name="fileName">Name of the file.</param>
        void SaveFile(Stream stream, string fileName);
    }
}