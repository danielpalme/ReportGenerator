using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly int maximumNumberOfHistoricCoverageFiles;

        /// <summary>
        /// The number reports that are parsed and processed in parallel.
        /// </summary>
        private readonly int numberOfReportsParsedInParallel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryParser" /> class.
        /// </summary>
        /// <param name="historyStorage">The history storage.</param>
        /// <param name="maximumNumberOfHistoricCoverageFiles">The maximum number of historic coverage files that get parsed.</param>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        internal HistoryParser(IHistoryStorage historyStorage, int maximumNumberOfHistoricCoverageFiles, int numberOfReportsParsedInParallel)
        {
            this.historyStorage = historyStorage ?? throw new ArgumentNullException(nameof(historyStorage));
            this.maximumNumberOfHistoricCoverageFiles = maximumNumberOfHistoricCoverageFiles;
            this.numberOfReportsParsedInParallel = numberOfReportsParsedInParallel;
        }

        /// <summary>
        /// Reads all historic coverage files created by <see cref="HistoryReportGenerator" /> and adds the information to all classes.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="overallHistoricCoverages">A list to which all history elements are added.</param>
        internal void ApplyHistoricCoverage(IEnumerable<Assembly> assemblies, List<HistoricCoverage> overallHistoricCoverages)
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
            object locker = new object();

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
                Logger.ErrorFormat(Resources.ErrorDuringReadingHistoricReports, ex.GetExceptionMessageForDisplay());
                return;
            }

            var classes = new Dictionary<string, Class>();

            foreach (var item in assemblies.SelectMany(t => t.Classes))
            {
                classes[this.GetFullClassName(item.Assembly.Name, item.RawName)] = item;
            }

            Parallel.ForEach(
                files,
                new ParallelOptions { MaxDegreeOfParallelism = this.numberOfReportsParsedInParallel },
                file =>
                {
                    try
                    {
                        Logger.InfoFormat(Resources.ParseHistoricFile, file);
                        var document = this.LoadXDocument(file);

                        DateTime date = DateTime.ParseExact(document.Root.Attribute("date").Value, "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                        string tag = document.Root.Attribute("tag")?.Value;
                        tag = string.IsNullOrEmpty(tag) ? null : tag;
                        var historicCoverages = this.ParseHistoricFile(classes, document, date, tag);
                        lock (locker)
                        {
                            overallHistoricCoverages.AddRange(historicCoverages);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(Resources.ErrorDuringReadingHistoricReport, file, ex.GetExceptionMessageForDisplay());
                    }
                });
        }

        private IEnumerable<HistoricCoverage> ParseHistoricFile(IDictionary<string, Class> classes, XDocument document, DateTime date, string tag)
        {
            ConcurrentBag<HistoricCoverage> historicCoverages = new ConcurrentBag<HistoricCoverage>();
            Parallel.ForEach(document.Root.Elements("assembly").ToArray(), assemblyElement =>
            {
                string assemblyName = assemblyElement.Attribute("name").Value;
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

                    var attribute = classElement.Attribute("coveredcodeelements");
                    if (attribute != null)
                    {
                        historicCoverage.CoveredCodeElements = int.Parse(attribute.Value, CultureInfo.InvariantCulture);
                    }

                    attribute = classElement.Attribute("fullcoveredcodeelements");
                    if (attribute != null)
                    {
                        historicCoverage.FullCoveredCodeElements = int.Parse(attribute.Value, CultureInfo.InvariantCulture);
                    }

                    attribute = classElement.Attribute("totalcodeelements");
                    if (attribute != null)
                    {
                        historicCoverage.TotalCodeElements = int.Parse(attribute.Value, CultureInfo.InvariantCulture);
                    }

                    historicCoverages.Add(historicCoverage);
                    if (classes.TryGetValue(this.GetFullClassName(assemblyName, classElement.Attribute("name").Value), out var @class))
                    {
                        @class.AddHistoricCoverage(historicCoverage);
                    }
                }
            });
            return historicCoverages;
        }

        private string GetFullClassName(string assemblyName, string className)
        {
            return $"{assemblyName}+{className}";
        }

        private XDocument LoadXDocument(string file)
        {
            using (var stream = this.historyStorage.LoadFile(file))
            {
                return XDocument.Load(stream);
            }
        }
    }
}