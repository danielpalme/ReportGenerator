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
    /// Preprocessor for JaCoCo reports.
    /// </summary>
    internal class JaCoCoReportPreprocessor
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(JaCoCoReportPreprocessor));

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IReadOnlyList<string> sourceDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="JaCoCoReportPreprocessor"/> class.
        /// </summary>
        /// <param name="sourceDirectories">The source directories.</param>
        internal JaCoCoReportPreprocessor(IEnumerable<string> sourceDirectories)
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
            if (this.sourceDirectories.Count == 0)
            {
                Logger.Warn("  " + string.Format(Resources.NoSourceDirectories, "JaCoCo"));
                return;
            }

            var modules = report.Descendants("package")
                .ToArray();

            foreach (var module in modules)
            {
                string moduleName = module.Attribute("name").Value;

                var sourcefilenameAttributesOfClasses = module.Elements("class")
                    .Select(e => e.Attribute("sourcefilename"))
                    .Where(e => e != null) // This attribute is not present in older JaCoCo versions
                    .ToArray();

                var nameAttributesOfSourceFiles = module.Elements("sourcefile")
                    .Select(e => e.Attribute("name"))
                    .ToArray();

                foreach (var attribute in sourcefilenameAttributesOfClasses)
                {
                    attribute.Value = this.GetFullFilePath(moduleName, attribute.Value);
                }

                foreach (var attribute in nameAttributesOfSourceFiles)
                {
                    attribute.Value = this.GetFullFilePath(moduleName, attribute.Value);
                }
            }
        }

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        /// <param name="moduleName">The name of the module/package.</param>
        /// <param name="fileName">The file.</param>
        /// <returns>The full path of the file.</returns>
        private string GetFullFilePath(string moduleName, string fileName)
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
                    .SelectMany(sourceDirectory => new[]
                    {
                        Path.Combine(sourceDirectory, fileName),
                        Path.Combine(sourceDirectory, moduleName, fileName)
                    })
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
