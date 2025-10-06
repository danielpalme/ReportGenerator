using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Markdown report renderer.
    /// </summary>
    internal class MarkdownRenderer : IMarkdownRenderer, IDisposable
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MarkdownRenderer));

        /// <summary>
        /// The current report text writer.
        /// </summary>
        private TextWriter reportTextWriter;

        /// <summary>
        /// The report text writer for the summary report.
        /// </summary>
        private TextWriter summaryReportTextWriter;

        /// <summary>
        /// The report text writer for the classes report.
        /// </summary>
        private TextWriter classReportTextWriter;

        /// <summary>
        /// The underlying stream for the summary report.
        /// </summary>
        private Stream summaryReportStream;

        /// <summary>
        /// The underlying stream for the classes report.
        /// </summary>
        private Stream classReportStream;

        /// <inheritdoc />
        public void BeginSummaryReport()
        {
            this.summaryReportStream = new MemoryStream();
            this.reportTextWriter = this.summaryReportTextWriter = new StreamWriter(this.summaryReportStream);
        }

        /// <inheritdoc />
        public void BeginClassReport(string targetDirectory, string classDisplayName)
        {
            if (this.classReportTextWriter == null)
            {
                string targetPath = Path.Combine(targetDirectory, "tmp.md");

                this.classReportStream = new FileStream(targetPath, FileMode.Create);
                this.classReportTextWriter = new StreamWriter(this.classReportStream);
            }

            this.reportTextWriter = this.classReportTextWriter;

            this.reportTextWriter.WriteLine($"# {classDisplayName}");
            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void Header(string text)
        {
            text = $"#{(this.reportTextWriter == this.classReportTextWriter ? "#" : string.Empty)} {text}";
            this.reportTextWriter.WriteLine(text);
            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void File(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.reportTextWriter.WriteLine($"### {path}");
        }

        /// <inheritdoc />
        public void Paragraph(string text)
        {
            this.reportTextWriter.WriteLine(text);
            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void BeginKeyValueTable()
        {
            this.reportTextWriter.WriteLine("|||");
            this.reportTextWriter.WriteLine("|:---|:---|");
        }

        /// <inheritdoc />
        public void BeginSummaryTable(bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            this.reportTextWriter.Write(
                "| **{0}** | **{1}** | **{2}** | **{3}** | **{4}** | **{5}** |",
                ReportResources.Name,
                ReportResources.Covered,
                ReportResources.Uncovered,
                ReportResources.Coverable,
                ReportResources.Total,
                ReportResources.Coverage);

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write($" **{ReportResources.Covered}** |");
                this.reportTextWriter.Write($" **{ReportResources.Total}** |");
                this.reportTextWriter.Write($" **{ReportResources.BranchCoverage}** |");
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write($" **{ReportResources.Covered}** |");
                this.reportTextWriter.Write($" **{ReportResources.Total}** |");
                this.reportTextWriter.Write($" **{ReportResources.CodeElementCoverageQuota}** |");
                this.reportTextWriter.Write($" **{ReportResources.FullCodeElementCoverageQuota}** |");
            }

            this.reportTextWriter.WriteLine();

            this.reportTextWriter.Write("|:---|---:|---:|---:|---:|---:|");

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write("---:|");
                this.reportTextWriter.Write("---:|");
                this.reportTextWriter.Write("---:|");
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write("---:|");
                this.reportTextWriter.Write("---:|");
                this.reportTextWriter.Write("---:|");
                this.reportTextWriter.Write("---:|");
            }

            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void BeginLineAnalysisBlock()
        {
            this.reportTextWriter.WriteLine("```");
        }

        /// <inheritdoc />
        public void FinishLineAnalysisBlock()
        {
            this.reportTextWriter.WriteLine("```");
        }

        /// <inheritdoc />
        public void KeyValueRow(string key, string value, bool emphasized)
        {
            if (emphasized)
            {
                this.reportTextWriter.WriteLine($"| **{key}** | {value} |");
            }
            else
            {
                this.reportTextWriter.WriteLine($"| {key} | {value} |");
            }
        }

        /// <inheritdoc />
        public void KeyValueRow(string key, IEnumerable<string> files)
        {
            this.reportTextWriter.WriteLine($"| **{key}** | {string.Join("<br />", files)} |");
        }

        /// <inheritdoc />
        public void MetricsTable(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var methodMetrics = @class.Files.SelectMany(f => f.MethodMetrics);

            this.MetricsTable(methodMetrics);
        }

        /// <inheritdoc />
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
            if (methodMetrics == null)
            {
                throw new ArgumentNullException(nameof(methodMetrics));
            }

            var metrics = methodMetrics
                .SelectMany(m => m.Metrics)
                .Distinct()
                .OrderBy(m => m.Name);

            int numberOfTables = (int)Math.Ceiling((double)metrics.Count() / 5);

            for (int i = 0; i < numberOfTables; i++)
            {
                string alignments = "|:---|" + string.Join("|", metrics.Skip(i * 5).Take(5).Select(m => "---:")) + "|";

                this.reportTextWriter.Write(@"| **" + ReportResources.Method + "** | " + string.Join(" | ", metrics.Skip(i * 5).Take(5).Select(m => @"**" + m.Name + "**")));
                this.reportTextWriter.WriteLine(" |");
                this.reportTextWriter.WriteLine(alignments);

                foreach (var methodMetric in methodMetrics.OrderBy(c => c.Line))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"| **{methodMetric.ShortName}** |");

                    foreach (var metric in metrics.Skip(i * 5).Take(5))
                    {
                        sb.Append(" ");

                        var metricValue = methodMetric.Metrics.FirstOrDefault(m => m.Equals(metric));

                        if (metricValue != null)
                        {
                            if (metricValue.Value.HasValue)
                            {
                                sb.Append(metricValue.Value.Value.ToString(CultureInfo.InvariantCulture));

                                if (metricValue.MetricType == MetricType.CoveragePercentual)
                                {
                                    sb.Append("%");
                                }
                            }
                            else
                            {
                                sb.Append("-");
                            }
                        }
                        else
                        {
                            sb.Append("-");
                        }

                        sb.Append(" |");
                    }

                    this.reportTextWriter.WriteLine(sb.ToString());
                }

                this.reportTextWriter.WriteLine();
            }
        }

        /// <inheritdoc />
        public void LineAnalysis(int fileIndex, LineAnalysis analysis, int maximumLineNumberDigits, int maximumLineVisitsDigits)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException(nameof(analysis));
            }

            string formattedLine = analysis.LineContent
                .Replace(((char)11).ToString(), "  ") // replace tab
                .Replace(((char)9).ToString(), "  "); // replace tab

            formattedLine = ShortenString(formattedLine, 120);
            formattedLine = StringHelper.ReplaceInvalidXmlChars(formattedLine);

            string lineVisitStatus;

            switch (analysis.LineVisitStatus)
            {
                case LineVisitStatus.Covered:
                    lineVisitStatus = "\u2714 ";
                    break;
                case LineVisitStatus.NotCovered:
                    lineVisitStatus = "\x274C";
                    break;
                case LineVisitStatus.PartiallyCovered:
                    lineVisitStatus = "\u2713 ";
                    break;
                default:
                    lineVisitStatus = "  ";
                    break;
            }

            string branchVisitStatus = " ";

            if (analysis.CoveredBranches.HasValue && analysis.TotalBranches.HasValue && analysis.TotalBranches.Value > 0)
            {
                int branchCoverage = (int)(100 * (double)analysis.CoveredBranches.Value / analysis.TotalBranches.Value);
                branchCoverage -= branchCoverage % 10;

                if (branchCoverage >= 100)
                {
                    branchVisitStatus = "\u25CF"; // Black Circle
                }
                else if (branchCoverage >= 75)
                {
                    branchVisitStatus = "\u25D5"; // Circle with Upper Right Quadrant Black
                }
                else if (branchCoverage >= 50)
                {
                    branchVisitStatus = "\u25D1"; // Circle with Left Half Black
                }
                else if (branchCoverage >= 25)
                {
                    branchVisitStatus = "\u25D4"; // Circle with Lower Right Quadrant Black
                }
                else
                {
                    branchVisitStatus = "\u25CB"; // White Circle
                }
            }

            string formatString = @"{0," + maximumLineNumberDigits + "}  {1} {2," + maximumLineVisitsDigits + "}  {3}  {4}";

            string row = string.Format(
                CultureInfo.InvariantCulture,
                formatString,
                analysis.LineNumber,
                lineVisitStatus,
                analysis.LineVisitStatus != LineVisitStatus.NotCoverable ? analysis.LineVisits.ToString(CultureInfo.InvariantCulture) : string.Empty,
                branchVisitStatus,
                formattedLine);

            this.reportTextWriter.WriteLine(row);
        }

        /// <inheritdoc />
        public void FinishTable()
        {
            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots, bool withLinks)
        {
            if (riskHotspots == null)
            {
                throw new ArgumentNullException(nameof(riskHotspots));
            }

            var codeQualityMetrics = riskHotspots.First().MethodMetric.Metrics
                .Where(m => m.MetricType == MetricType.CodeQuality)
                .ToArray();

            string alignments = "|:---|:---|:---|" + string.Join("|", codeQualityMetrics.Select(m => "---:")) + "|";

            this.reportTextWriter.Write(
                "| **{0}** | **{1}** | **{2}** |",
                ReportResources.Assembly2,
                ReportResources.Class2,
                ReportResources.Method);

            foreach (var metric in codeQualityMetrics)
            {
                this.reportTextWriter.Write(" **{0}** |", metric.Name);
            }

            this.reportTextWriter.WriteLine();

            this.reportTextWriter.WriteLine(alignments);

            foreach (var riskHotspot in riskHotspots)
            {
                string className = withLinks
                ? $"[{riskHotspot.Class.DisplayName}](#{riskHotspot.Class.DisplayName.ToLowerInvariant().Replace(".", string.Empty).Replace(" ", "-")})"
                : riskHotspot.Class.DisplayName;

                this.reportTextWriter.Write(
                    "| {0} | {1} | {2} |",
                    riskHotspot.Assembly.ShortName,
                    className,
                    riskHotspot.MethodMetric.ShortName);

                foreach (var statusMetric in riskHotspot.StatusMetrics)
                {
                    this.reportTextWriter.Write(" {0} |", statusMetric.Metric.Value.HasValue ? statusMetric.Metric.Value.Value.ToString(CultureInfo.InvariantCulture) : "-");
                }
            }

            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"| **{0}** | **{1}** | **{2}** | **{3}** | **{4}** | **{5}** |",
                assembly.Name,
                assembly.CoveredLines,
                assembly.CoverableLines - assembly.CoveredLines,
                assembly.CoverableLines,
                assembly.TotalLines.GetValueOrDefault(),
                assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

            this.reportTextWriter.Write(row);

            if (branchCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" **{0}** | **{1}** | **{2}** |",
                    assembly.CoveredBranches,
                    assembly.TotalBranches,
                    assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            if (methodCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" **{0}** | **{1}** | **{2}** | **{3}** |",
                    assembly.CoveredCodeElements,
                    assembly.TotalCodeElements,
                    assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty,
                    assembly.FullCodeElementCoverageQuota.HasValue ? assembly.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void SummaryClass(Class @class, bool branchCoverageAvailable, bool methodCoverageAvailable, bool withLinks)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            string className = withLinks
                ? $"[{@class.DisplayName}](#{@class.DisplayName.ToLowerInvariant().Replace(".", string.Empty).Replace(" ", "-")})"
                : @class.DisplayName;

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"| {0} | {1} | {2} | {3} | {4} | {5} |",
                className,
                @class.CoveredLines,
                @class.CoverableLines - @class.CoveredLines,
                @class.CoverableLines,
                @class.TotalLines.GetValueOrDefault(),
                @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

            this.reportTextWriter.Write(row);

            if (branchCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" {0} | {1} | {2} |",
                    @class.CoveredBranches,
                    @class.TotalBranches,
                    @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            if (methodCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" {0} | {1} | {2} | {3} |",
                    @class.CoveredCodeElements,
                    @class.TotalCodeElements,
                    @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty,
                    @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            this.reportTextWriter.WriteLine();
        }

        /// <inheritdoc />
        public void SaveSummaryReport(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, "Summary.md");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            Stream combinedReportStream = new FileStream(targetPath, FileMode.Create);

            this.summaryReportTextWriter.Flush();
            this.summaryReportStream.Position = 0;
            this.summaryReportStream.CopyTo(combinedReportStream);

            this.summaryReportTextWriter.Dispose();

            if (this.classReportTextWriter != null)
            {
                this.classReportTextWriter.Flush();
                this.classReportStream.Position = 0;
                this.classReportStream.CopyTo(combinedReportStream);

                this.classReportTextWriter.Dispose();

                System.IO.File.Delete(Path.Combine(targetDirectory, "tmp.md"));
            }

            combinedReportStream.Flush();
            combinedReportStream.Dispose();

            this.classReportTextWriter = null;
            this.summaryReportTextWriter = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.summaryReportTextWriter != null)
                {
                    this.summaryReportTextWriter.Dispose();
                }

                if (this.classReportTextWriter != null)
                {
                    this.classReportTextWriter.Dispose();
                }

                if (this.summaryReportStream != null)
                {
                    this.summaryReportStream.Dispose();
                }

                if (this.classReportStream != null)
                {
                    this.classReportStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Shortens the given string.
        /// </summary>
        /// <param name="text">The text to shorten.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>The shortened string.</returns>
        private static string ShortenString(string text, int maxLength)
        {
            if (text.Length > maxLength)
            {
                return text.Substring(0, maxLength);
            }

            return text;
        }
    }
}
