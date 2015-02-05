using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using log4net;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Generates historic report containing coverage information of current test run.
    /// This information is used to create historic charts in reports.
    /// </summary>
    internal class HistoryReportGenerator
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HistoryReportGenerator));

        /// <summary>
        /// The parser to use.
        /// </summary>
        private readonly IParser parser;

        /// <summary>
        /// The history directory.
        /// </summary>
        private readonly string historyDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryReportGenerator"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="historyDirectory">The history directory.</param>
        internal HistoryReportGenerator(IParser parser, string historyDirectory)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            if (historyDirectory == null)
            {
                throw new ArgumentNullException("historyDirectory");
            }

            this.parser = parser;
            this.historyDirectory = historyDirectory;
        }

        /// <summary>
        /// Starts the generation of the report.
        /// </summary>
        /// <param name="executionTime">The execution time.</param>
        internal void CreateReport(DateTime executionTime)
        {
            Logger.Info(Resources.CreatingHistoryReport);

            string date = executionTime.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

            var rootElement = new XElement(
                "coverage",
                new XAttribute("version", "1.0"),
                new XAttribute("date", date));

            foreach (var assembly in this.parser.Assemblies)
            {
                var assemblyElement = new XElement(
                    "assembly",
                    new XAttribute("name", assembly.Name));

                foreach (var @class in assembly.Classes)
                {
                    var classElement = new XElement(
                       "class",
                       new XAttribute("name", @class.Name),
                       new XAttribute("coveredlines", @class.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("coverablelines", @class.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("totallines", @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("coveredbranches", @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("totalbranches", @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));

                    assemblyElement.Add(classElement);
                }

                rootElement.Add(assemblyElement);
            }

            var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), rootElement);
            document.Save(Path.Combine(this.historyDirectory, date + "_CoverageHistory.xml"));
        }
    }
}