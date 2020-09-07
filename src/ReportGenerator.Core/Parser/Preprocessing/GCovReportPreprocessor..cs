using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for GCov reports.
    /// </summary>
    internal class GCovReportPreprocessor
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(GCovReportPreprocessor));

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IReadOnlyList<string> sourceDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="GCovReportPreprocessor"/> class.
        /// </summary>
        /// <param name="sourceDirectories">The source directories.</param>
        internal GCovReportPreprocessor(IEnumerable<string> sourceDirectories)
        {
            if (sourceDirectories == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectories));
            }

            this.sourceDirectories = sourceDirectories.ToList();
        }

        /// <summary>
        /// Executes the preprocessing of the report.
        /// </summary>
        /// <param name="lines">The report lines.</param>
        internal void Execute(string[] lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (this.sourceDirectories.Count == 0)
            {
                Logger.Warn("  " + string.Format(Resources.NoSourceDirectories, "GCov"));
                return;
            }

            if (lines.Length == 0)
            {
                return;
            }

            string fileName = lines[0].Substring(lines[0].IndexOf(GCovParser.SourceElementInFirstLine) + GCovParser.SourceElementInFirstLine.Length);

            lines[0] = GCovParser.SourceElementInFirstLine + this.GetFullFilePath(fileName);
        }

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        /// <param name="fileName">The file.</param>
        /// <returns>The full path of the file.</returns>
        private string GetFullFilePath(string fileName)
        {
            if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
            {
                return fileName;
            }

            fileName = fileName
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            if (!Path.IsPathRooted(fileName))
            {
                var path = this.sourceDirectories
                    .Select(sourceDirectory => Path.Combine(sourceDirectory, fileName))
                    .Select(p => p
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar))
                    .FirstOrDefault(p => File.Exists(p));

                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }

            return fileName;
        }
    }
}
