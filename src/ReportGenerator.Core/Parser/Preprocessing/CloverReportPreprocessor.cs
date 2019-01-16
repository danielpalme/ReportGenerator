using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for Clover reports.
    /// </summary>
    internal class CloverReportPreprocessor
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CloverReportPreprocessor));

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IReadOnlyList<string> sourceDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloverReportPreprocessor"/> class.
        /// </summary>
        /// <param name="sourceDirectories">The source directories.</param>
        internal CloverReportPreprocessor(IEnumerable<string> sourceDirectories)
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
        /// <param name="report">The report.</param>
        internal void Execute(XContainer report)
        {
            var files = report.Descendants("package").Elements("file").ToArray();

            if (this.sourceDirectories.Count == 0)
            {
                foreach (var file in files)
                {
                    if (!Path.IsPathRooted(file.Attribute("path").Value))
                    {
                        Logger.Warn("  " + string.Format(Resources.NoSouceDirectories, "Clover"));
                        return;
                    }
                }

                return;
            }

            foreach (var file in files)
            {
                var pathAttribute = file.Attribute("path");

                pathAttribute.Value = this.GetFullFilePath(pathAttribute.Value);
            }
        }

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        /// <param name="initialPath">The initial path of the file.</param>
        /// <returns>The full path of the file.</returns>
        private string GetFullFilePath(string initialPath)
        {
            if (!Path.IsPathRooted(initialPath))
            {
                foreach (var sourceDirectory in this.sourceDirectories)
                {
                    string path = Path.Combine(sourceDirectory, initialPath);

                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return initialPath;
        }
    }
}
