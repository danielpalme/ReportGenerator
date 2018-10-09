using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.History
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
        /// The maximum number of historic coverage files that get parsed.
        /// </summary>
        private int maximumNumberOfHistoricCoverageFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryParser" /> class.
        /// </summary>
        /// <param name="historyStorage">The history storage.</param>
        /// <param name="maximumNumberOfHistoricCoverageFiles">The maximum number of historic coverage files that get parsed.</param>
        internal HistoryParser(IHistoryStorage historyStorage, int maximumNumberOfHistoricCoverageFiles)
        {
            this.historyStorage = historyStorage ?? throw new ArgumentNullException(nameof(historyStorage));
            this.maximumNumberOfHistoricCoverageFiles = maximumNumberOfHistoricCoverageFiles;
        }

        /// <summary>
        /// Reads all historic coverage files created by <see cref="HistoryReportGenerator" /> and adds the information to all classes.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="overallHistoricCoverages">A list to which all history elements are added.</param>
        internal void ApplyHistoricCoverage(IEnumerable<Assembly> assemblies, IList<HistoricCoverage> overallHistoricCoverages)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (overallHistoricCoverages == null)
            {
                throw new ArgumentNullException(nameof(overallHistoricCoverages));
            }

            Logger.Info(Resources.ReadingHistoricReports);

            IEnumerable<string> files = null;

            try
            {
                files = this.historyStorage.GetHistoryFilePaths()
                    .OrderByDescending(f => f)
                    .Take(this.maximumNumberOfHistoricCoverageFiles)
                    .Reverse()
                    .ToArray();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(" " + Resources.ErrorDuringReadingHistoricReports, ex.GetExceptionMessageForDisplay());
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
                    string tag = document.Root.Attribute("tag")?.Value;
                    tag = string.IsNullOrEmpty(tag) ? null : tag;

                    foreach (var assemblyElement in document.Root.Elements("assembly"))
                    {
                        Assembly assembly = assemblies
                            .SingleOrDefault(a => a.Name == assemblyElement.Attribute("name").Value);

                        foreach (var classElement in assemblyElement.Elements("class"))
                        {
                            HistoricCoverage historicCoverage = new HistoricCoverage(date, tag)
                            {
                                CoveredLines = int.Parse(classElement.Attribute("coveredlines").Value, CultureInfo.InvariantCulture),
                                CoverableLines = int.Parse(classElement.Attribute("coverablelines").Value, CultureInfo.InvariantCulture),
                                TotalLines = int.Parse(classElement.Attribute("totallines").Value, CultureInfo.InvariantCulture),
                                CoveredBranches = int.Parse(classElement.Attribute("coveredbranches").Value, CultureInfo.InvariantCulture),
                                TotalBranches = int.Parse(classElement.Attribute("totalbranches").Value, CultureInfo.InvariantCulture)
                            };

                            overallHistoricCoverages.Add(historicCoverage);

                            if (assembly == null)
                            {
                                continue;
                            }

                            Class @class = assembly.Classes
                                .SingleOrDefault(c => c.Name == classElement.Attribute("name").Value);

                            if (@class == null)
                            {
                                continue;
                            }

                            @class.AddHistoricCoverage(historicCoverage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(" " + Resources.ErrorDuringReadingHistoricReport, file, ex.GetExceptionMessageForDisplay());
                }
            }
        }
    }
}