using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.History
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
        /// Optional custom file prefix.
        /// </summary>
        private readonly string customfilePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryReportGenerator" /> class.
        /// </summary>
        /// <param name="historyStorage">The history storage.</param>
        /// <param name="customfilePrefix">Optional custom file prefix.</param>
        internal HistoryReportGenerator(IHistoryStorage historyStorage, string customfilePrefix)
        {
            this.historyStorage = historyStorage ?? throw new ArgumentNullException(nameof(historyStorage));
            this.customfilePrefix = string.IsNullOrWhiteSpace(customfilePrefix) ? string.Empty : "_" + customfilePrefix;
        }

        /// <summary>
        /// Starts the generation of the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="executionTime">The execution time.</param>
        /// <param name="tag">The custom tag (e.g. build number).</param>
        internal void CreateReport(IEnumerable<Assembly> assemblies, DateTime executionTime, string tag)
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
                new XAttribute("date", date),
                new XAttribute("tag", tag ?? string.Empty));

            foreach (var assembly in assemblies)
            {
                var assemblyElement = new XElement(
                    "assembly",
                    new XAttribute("name", assembly.Name));

                foreach (var @class in assembly.Classes)
                {
                    var classElement = new XElement(
                       "class",
                       new XAttribute("name", @class.RawName),
                       new XAttribute("coveredlines", @class.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("coverablelines", @class.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("totallines", @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("coveredbranches", @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("totalbranches", @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("coveredcodeelements", @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("fullcoveredcodeelements", @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)),
                       new XAttribute("totalcodeelements", @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)));

                    assemblyElement.Add(classElement);
                }

                rootElement.Add(assemblyElement);
            }

            var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), rootElement);
            string fileName = date + this.customfilePrefix + "_CoverageHistory.xml";
            try
            {
                using (var stream = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        Encoding = new UTF8Encoding(false),
                        Indent = true
                    };

                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        document.Save(writer);
                    }

                    stream.Position = 0;
                    this.historyStorage.SaveFile(stream, fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(Resources.ErrorDuringSavingHistoricReport, fileName, ex.GetExceptionMessageForDisplay());
            }
        }
    }
}