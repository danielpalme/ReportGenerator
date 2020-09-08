using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in JSON format (no reports for classes are generated).
    /// </summary>
    public class JsonSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(JsonSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public virtual string ReportType => "JsonSummary";

        /// <summary>
        /// Gets or sets the report context.
        /// </summary>
        /// <value>
        /// The report context.
        /// </value>
        public IReportContext ReportContext { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public virtual void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
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

            string targetPath = Path.Combine(targetDirectory, "Summary.json");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create), Encoding.UTF8))
            {
                reportTextWriter.WriteLine("{");
                reportTextWriter.WriteLine("  \"summary\": {");
                reportTextWriter.WriteLine($"    \"generatedon\": \"{DateTime.Now.ToUniversalTime().ToString("s")}Z\",");
                reportTextWriter.WriteLine($"    \"parser\": \"{summaryResult.UsedParser}\",");
                reportTextWriter.WriteLine($"    \"assemblies\": {summaryResult.Assemblies.Count().ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"classes\": {summaryResult.Assemblies.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"files\": {summaryResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"coveredlines\": {summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"uncoveredlines\": {(summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"coverablelines\": {summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"totallines\": {summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},");

                if (summaryResult.CoverageQuota.HasValue)
                {
                    reportTextWriter.Write($"    \"linecoverage\": {summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}");
                }
                else
                {
                    reportTextWriter.Write($"    \"linecoverage\": null");
                }

                if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
                {
                    reportTextWriter.WriteLine(",");
                    reportTextWriter.WriteLine($"    \"coveredbranches\": {summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},");
                    reportTextWriter.Write($"    \"totalbranches\": {summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}");

                    if (summaryResult.BranchCoverageQuota.HasValue)
                    {
                        reportTextWriter.WriteLine(",");
                        reportTextWriter.Write($"    \"branchcoverage\": {summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                reportTextWriter.WriteLine(" },");

                var sumableMetrics = summaryResult.SumableMetrics;

                if (sumableMetrics.Count > 0)
                {
                    reportTextWriter.WriteLine("  \"metrics\": [");

                    int metricCounter = 0;

                    foreach (var metric in sumableMetrics)
                    {
                        if (metricCounter > 0)
                        {
                            reportTextWriter.WriteLine(",");
                        }

                        if (metric.Value.HasValue)
                        {
                            reportTextWriter.Write($"    {{ \"name\": \"{JsonSerializer.EscapeString(metric.Name)}\", \"value\": {metric.Value.Value.ToString(CultureInfo.InvariantCulture)} }}");
                        }
                        else
                        {
                            reportTextWriter.Write($"    {{ \"name\": \"{JsonSerializer.EscapeString(metric.Name)}\", \"value\": null }}");
                        }

                        metricCounter++;
                    }

                    reportTextWriter.WriteLine(" ],");
                }

                reportTextWriter.WriteLine("  \"coverage\": {");

                reportTextWriter.WriteLine("    \"assemblies\": [");

                int assemblyCounter = 0;

                foreach (var assembly in summaryResult.Assemblies)
                {
                    if (assemblyCounter > 0)
                    {
                        reportTextWriter.WriteLine(",");
                    }

                    reportTextWriter.WriteLine($"      {{ \"name\": \"{JsonSerializer.EscapeString(assembly.Name)}\", \"classes\": {assembly.Classes.Count().ToString(CultureInfo.InvariantCulture)}, \"coverage\": {(assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"coveredlines\": {assembly.CoveredLines.ToString(CultureInfo.InvariantCulture)}, \"coverablelines\": {assembly.CoverableLines.ToString(CultureInfo.InvariantCulture)}, \"totallines\": {(assembly.TotalLines.HasValue ? assembly.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"branchcoverage\": {(assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"coveredbranches\": {(assembly.CoveredBranches.HasValue ? assembly.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"totalbranches\": {(assembly.TotalBranches.HasValue ? assembly.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : null)}, \"classesinassembly\": [");

                    int classCounter = 0;

                    foreach (var @class in assembly.Classes)
                    {
                        if (classCounter > 0)
                        {
                            reportTextWriter.WriteLine(",");
                        }

                        reportTextWriter.Write($"        {{ \"name\": \"{JsonSerializer.EscapeString(@class.Name)}\", \"coverage\": {(@class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"coveredlines\": {@class.CoveredLines.ToString(CultureInfo.InvariantCulture)}, \"coverablelines\": {@class.CoverableLines.ToString(CultureInfo.InvariantCulture)}, \"totallines\": {(@class.TotalLines.HasValue ? @class.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"branchcoverage\": {(@class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"coveredbranches\": {(@class.CoveredBranches.HasValue ? @class.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : "null")}, \"totalbranches\": {(@class.TotalBranches.HasValue ? @class.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : null)} }}");

                        classCounter++;
                    }

                    reportTextWriter.Write(" ] }");

                    assemblyCounter++;
                }

                reportTextWriter.WriteLine(" ]");

                reportTextWriter.WriteLine("  }");
                reportTextWriter.Write("}");

                reportTextWriter.Flush();
            }
        }
    }
}
