using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.FileReading
{
    /// <summary>
    /// File reader for reading files from local disk.
    /// </summary>
    internal class LocalFileReader : IFileReader
    {
        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IReadOnlyList<string> sourceDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileReader" /> class.
        /// </summary>
        public LocalFileReader()
            : this(Enumerable.Empty<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileReader" /> class.
        /// </summary>
        /// <param name="sourceDirectories">The source directories.</param>
        public LocalFileReader(IEnumerable<string> sourceDirectories)
        {
            if (sourceDirectories == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectories));
            }

            this.sourceDirectories = sourceDirectories.ToList();
        }

        /// <summary>
        /// Loads the file with the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="error">Error message if file reading failed, otherwise <code>null</code>.</param>
        /// <returns><code>null</code> if an error occurs, otherwise the lines of the file.</returns>
        public string[] LoadFile(string path, out string error)
        {
            path = this.MapPath(path);

            try
            {
                if (!File.Exists(path))
                {
                    error = string.Format(CultureInfo.InvariantCulture, Resources.FileDoesNotExist, path);
                    return null;
                }

                var encoding = FileHelper.GetEncoding(path);
                string[] lines = File.ReadAllLines(path, encoding);

                error = null;
                return lines;
            }
            catch (Exception ex)
            {
                error = string.Format(CultureInfo.InvariantCulture, Resources.ErrorDuringReadingFile, path, ex.GetExceptionMessageForDisplay());
                return null;
            }
        }

        private string MapPath(string path)
        {
            if (this.sourceDirectories.Count == 0 || File.Exists(path))
            {
                return path;
            }

            string[] parts = path.Split('/', '\\');

            foreach (var sourceDirectory in this.sourceDirectories)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    string combinedPath = sourceDirectory;

                    for (int j = i; j < parts.Length; j++)
                    {
                        combinedPath = Path.Combine(combinedPath, parts[j]);
                    }

                    if (File.Exists(combinedPath))
                    {
                        return combinedPath;
                    }
                }
            }

            return path;
        }
    }
}
