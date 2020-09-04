using System;
using System.Collections.Generic;
using System.IO;

namespace Palmmedia.ReportGenerator.Core.Reporting.History
{
    /// <summary>
    /// Default implementation of <see cref="IHistoryStorage"/> based on file system.
    /// </summary>
    internal class FileHistoryStorage : IHistoryStorage
    {
        /// <summary>
        /// The history directory.
        /// </summary>
        private readonly string historyDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHistoryStorage"/> class.
        /// </summary>
        /// <param name="historyDirectory">The history directory.</param>
        public FileHistoryStorage(string historyDirectory)
        {
            this.historyDirectory = historyDirectory;
        }

        /// <summary>
        /// Gets the history file paths.
        /// </summary>
        /// <returns>
        /// The history file paths.
        /// </returns>
        public IEnumerable<string> GetHistoryFilePaths() => Directory.EnumerateFiles(this.historyDirectory, "*_CoverageHistory.xml");

        /// <summary>
        /// Loads the given file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// The file as stream.
        /// </returns>
        public Stream LoadFile(string filePath) => File.OpenRead(filePath);

        /// <summary>
        /// Saves the file with the given name.
        /// </summary>
        /// <param name="stream">The stream containing the file content.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SaveFile(Stream stream, string fileName)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            using (var output = File.OpenWrite(Path.Combine(this.historyDirectory, fileName)))
            {
                stream.CopyTo(output);
            }
        }
    }
}
