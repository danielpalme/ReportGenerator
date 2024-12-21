using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in CSV format (no reports for classes are generated).
    /// </summary>
    public class CsvSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CsvSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "CsvSummary";

        /// <summary>
        /// Gets or sets the report configuration.
        /// </summary>
        /// <value>
        /// The report configuration.
        /// </value>
        public IReportContext ReportContext { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

            string targetDirectory = this.ReportContext.ReportConfiguration.TargetDirectory;

            if (this.ReportContext.Settings.CreateSubdirectoryForAllReportTypes)
            {
                targetDirectory = Path.Combine(targetDirectory, this.ReportType);

                if (!Directory.Exists(targetDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(Resources.TargetDirectoryCouldNotBeCreated, targetDirectory, ex.GetExceptionMessageForDisplay());
                        return;
                    }
                }
            }

            string targetPath = Path.Combine(targetDirectory, "Summary.csv");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = File.CreateText(targetPath))
            {
                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                reportTextWriter.WriteLine(ReportResources.Summary);
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.GeneratedOn,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString());
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Parser,
                    summaryResult.UsedParser);
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Assemblies2,
                    assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Classes,
                    assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Files2,
                    assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Coverage2,
                    summaryResult.CoverageQuota.HasValue ? summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.CoveredLines,
                    summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.UncoveredLines,
                    (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.CoverableLines,
                    summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.TotalLines,
                    summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                foreach (var assembly in assembliesWithClasses)
                {
                    reportTextWriter.WriteLine();
                    reportTextWriter.WriteLine(
                        "{0};{1}",
                        assembly.Name,
                        assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                    if (assembly.Classes.Any())
                    {
                        reportTextWriter.WriteLine();
                    }

                    foreach (var @class in assembly.Classes)
                    {
                        reportTextWriter.WriteLine(
                            "{0};{1}",
                            @class.Name,
                            @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                    }
                }

                reportTextWriter.Flush();
            }
        }
    }
}
