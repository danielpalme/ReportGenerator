using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting.History
{
    /// <summary>
    /// Reads all historic coverage files created by <see cref="HistoryReportGenerator"/> and adds the information to all classes.
    /// </summary>
    internal class HistoryParser
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HistoryParser));

        /// <summary>
        /// The history storage.
        /// </summary>
        private readonly IHistoryStorage historyStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryParser" /> class.
        /// </summary>
        /// <param name="historyStorage">The history storage.</param>
        internal HistoryParser(IHistoryStorage historyStorage)
        {
            if (historyStorage == null)
            {
                throw new ArgumentNullException(nameof(historyStorage));
            }

            this.historyStorage = historyStorage;
        }

        /// <summary>
        /// Reads all historic coverage files created by <see cref="HistoryReportGenerator" /> and adds the information to all classes.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        internal void ApplyHistoricCoverage(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            Logger.Info(Resources.ReadingHistoricReports);

            IEnumerable<string> files = null;

            try
            {
                files = this.historyStorage.GetHistoryFilePaths()
                    .OrderByDescending(f => f)
                    .Take(Settings.Default.MaximumOfHistoricCoverageFiles)
                    .Reverse()
                    .ToArray();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(" " + Resources.ErrorDuringReadingHistoricReports, ex.Message);
                return;
            }

            foreach (var file in files)
            {
                try
                {
                    XDocument document = null;

                    using (var stream = this.historyStorage.LoadFile(file))
                    {
                        document = XDocument.Load(stream);
                    }

                    DateTime date = DateTime.ParseExact(document.Root.Attribute("date").Value, "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

                    foreach (var assemblyElement in document.Root.Elements("assembly"))
                    {
                        Assembly assembly = assemblies
                            .SingleOrDefault(a => a.Name == assemblyElement.Attribute("name").Value);

                        if (assembly == null)
                        {
                            continue;
                        }

                        foreach (var classElement in assemblyElement.Elements("class"))
                        {
                            Class @class = assembly.Classes
                                .SingleOrDefault(c => c.Name == classElement.Attribute("name").Value);

                            if (@class == null)
                            {
                                continue;
                            }

                            HistoricCoverage historicCoverage = new HistoricCoverage(date)
                            {
                                CoveredLines = int.Parse(classElement.Attribute("coveredlines").Value, CultureInfo.InvariantCulture),
                                CoverableLines = int.Parse(classElement.Attribute("coverablelines").Value, CultureInfo.InvariantCulture),
                                TotalLines = int.Parse(classElement.Attribute("totallines").Value, CultureInfo.InvariantCulture),
                                CoveredBranches = int.Parse(classElement.Attribute("coveredbranches").Value, CultureInfo.InvariantCulture),
                                TotalBranches = int.Parse(classElement.Attribute("totalbranches").Value, CultureInfo.InvariantCulture)
                            };

                            @class.AddHistoricCoverage(historicCoverage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(" " + Resources.ErrorDuringReadingHistoricReport, file, ex.Message);
                }
            }
        }
    }
}