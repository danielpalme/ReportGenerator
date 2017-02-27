using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates summary report in CSV format (no reports for classes are generated).
    /// </summary>
    [System.ComponentModel.Composition.Export(typeof(IReportBuilder))]
    public class CsvSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "CsvSummary";

        /// <summary>
        /// Gets or sets the target directory where reports are stored.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        public string TargetDirectory { get; set; }

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

            string targetPath = Path.Combine(this.TargetDirectory, "Summary.csv");

            using (var reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create), Encoding.UTF8))
            {
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
                    summaryResult.Assemblies.Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Classes,
                    summaryResult.Assemblies.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Files2,
                    summaryResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine(
                    "{0};{1}",
                    ReportResources.Coverage2,
                    summaryResult.CoverageQuota.HasValue ? summaryResult.CoverageQuota.Value.ToString("f1", CultureInfo.InvariantCulture) + "%" : string.Empty);
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

                foreach (var assembly in summaryResult.Assemblies)
                {
                    reportTextWriter.WriteLine();
                    reportTextWriter.WriteLine(
                        "{0};{1}",
                        assembly.Name,
                        assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString("f1", CultureInfo.InvariantCulture) + "%" : string.Empty);

                    if (assembly.Classes.Any())
                    {
                        reportTextWriter.WriteLine();
                    }

                    foreach (var @class in assembly.Classes)
                    {
                        reportTextWriter.WriteLine(
                            "{0};{1}",
                            @class.Name,
                            @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString("f1", CultureInfo.InvariantCulture) + "%" : string.Empty);
                    }
                }

                reportTextWriter.Flush();
            }
        }
    }
}
