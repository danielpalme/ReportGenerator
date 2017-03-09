using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting.History
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
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HistoryReportGenerator));

        /// <summary>
        /// The history storage.
        /// </summary>
        private readonly IHistoryStorage historyStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryReportGenerator" /> class.
        /// </summary>
        /// <param name="historyStorage">The history storage.</param>
        internal HistoryReportGenerator(IHistoryStorage historyStorage)
        {
            if (historyStorage == null)
            {
                throw new ArgumentNullException(nameof(historyStorage));
            }

            this.historyStorage = historyStorage;
        }

        /// <summary>
        /// Starts the generation of the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="executionTime">The execution time.</param>
        internal void CreateReport(IEnumerable<Assembly> assemblies, DateTime executionTime)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            Logger.Info(Resources.CreatingHistoryReport);

            string date = executionTime.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

            var rootElement = new XElement(
                "coverage",
                new XAttribute("version", "1.0"),
                new XAttribute("date", date));

            foreach (var assembly in assemblies)
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
            string fileName = date + "_CoverageHistory.xml";
            try
            {
                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    stream.Position = 0;
                    this.historyStorage.SaveFile(stream, fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(" " + Resources.ErrorDuringSavingHistoricReport, fileName, ex.Message);
            }
        }
    }
}