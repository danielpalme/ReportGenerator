using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for Cobertura reports.
    /// </summary>
    internal class CoberturaReportPreprocessor
    {
        /// <summary>
        /// The report file as XContainer.
        /// </summary>
        private readonly XContainer report;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoberturaReportPreprocessor"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        internal CoberturaReportPreprocessor(XContainer report)
        {
            this.report = report;
        }

        /// <summary>
        /// Executes the preprocessing of the report.
        /// </summary>
        internal void Execute()
        {
            var sources = this.report.Descendants("sources")
                .Elements("source")
                .Select(s => s.Value)
                .ToArray();

            if (sources.Length == 0)
            {
                return;
            }

            // Issue 115: https://stackoverflow.com/questions/19909008/path-combine-does-not-add-directory-separator-after-drive-letter 
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].Length == 2 && sources[i][1] == ':')
                {
                    sources[i] = sources[i] + System.IO.Path.DirectorySeparatorChar;
                }
            }

            var classes = this.report.Descendants("package")
                .Elements("classes")
                .Elements("class")
                .ToArray();

            if (sources.Length == 1)
            {
                foreach (var @class in classes)
                {
                    var fileNameAttribute = @class.Attribute("filename");
                    string path = Path.Combine(sources[0], fileNameAttribute.Value)
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar);
                    fileNameAttribute.Value = path;
                }
            }
            else
            {
                foreach (var @class in classes)
                {
                    foreach (var source in sources)
                    {
                        var fileNameAttribute = @class.Attribute("filename");
                        string path = Path.Combine(sources[0], fileNameAttribute.Value)
                            .Replace('\\', Path.DirectorySeparatorChar)
                            .Replace('/', Path.DirectorySeparatorChar);

                        if (File.Exists(path))
                        {
                            fileNameAttribute.Value = path;
                            break;
                        }
                    }
                }
            }
        }
    }
}
