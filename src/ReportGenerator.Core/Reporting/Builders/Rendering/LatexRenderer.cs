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
    /// Latex report renderer.
    /// </summary>
    internal class LatexRenderer : ILatexRenderer, IDisposable
    {
        /// <summary>
        /// The head of each generated Latex file.
        /// </summary>
        private const string LatexStart = @"\documentclass[a4paper,landscape,10pt]{{article}}
\usepackage[paper=a4paper,landscape,left=20mm,right=20mm,top=20mm,bottom=20mm]{{geometry}}
\usepackage{{longtable}}
\usepackage{{fancyhdr}}
\usepackage[pdftex]{{color}}
\usepackage{{colortbl}}
\definecolor{{green}}{{rgb}}{{0.04,0.68,0.04}}
\definecolor{{orange}}{{rgb}}{{0.97,0.65,0.12}}
\definecolor{{red}}{{rgb}}{{0.75,0.04,0.04}}
\definecolor{{gray}}{{rgb}}{{0.86,0.86,0.86}}

\usepackage[pdftex,
            colorlinks=true, linkcolor=red, urlcolor=green, citecolor=red,%
            raiselinks=true,%
            bookmarks=true,%
            bookmarksopenlevel=1,%
            bookmarksopen=true,%
            bookmarksnumbered=true,%
            hyperindex=true,% 
            plainpages=false,% correct hyperlinks
            pdfpagelabels=true%,% view TeX pagenumber in PDF reader
            %pdfborder={{0 0 0.5}}
            ]{{hyperref}}

\hypersetup{{pdftitle={{{0}}},
            pdfauthor={{ReportGenerator - {1}}}
           }}

\pagestyle{{fancy}}
\fancyhead[LE,LO]{{\leftmark}}
\fancyhead[R]{{\thepage}}
\fancyfoot[C]{{ReportGenerator - {1}}}

\begin{{document}}

\setcounter{{secnumdepth}}{{-1}}";

        /// <summary>
        /// The end of each generated Latex file.
        /// </summary>
        private const string LatexEnd = @"\end{document}";

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(LatexRenderer));

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

            string start = string.Format(
                CultureInfo.InvariantCulture,
                LatexStart,
                EscapeLatexChars(ReportResources.CoverageReport),
                typeof(IReportBuilder).Assembly.GetName().Version);

            this.reportTextWriter.WriteLine(start);
        }

        /// <inheritdoc />
        public void BeginClassReport(string targetDirectory, string classDisplayName)
        {
            if (this.classReportTextWriter == null)
            {
                string targetPath = Path.Combine(targetDirectory, "tmp.tex");

                this.classReportStream = new FileStream(targetPath, FileMode.Create);
                this.classReportTextWriter = new StreamWriter(this.classReportStream);
            }

            this.reportTextWriter = this.classReportTextWriter;
            this.reportTextWriter.WriteLine(@"\newpage");

            classDisplayName = string.Format(CultureInfo.InvariantCulture, @"\section{{{0}}}", EscapeLatexChars(classDisplayName));
            this.reportTextWriter.WriteLine(classDisplayName);
        }

        /// <inheritdoc />
        public void Header(string text)
        {
            text = string.Format(
                CultureInfo.InvariantCulture,
                @"\{0}section{{{1}}}",
                this.reportTextWriter == this.classReportTextWriter ? "sub" : string.Empty,
                EscapeLatexChars(text));
            this.reportTextWriter.WriteLine(text);
        }

        /// <inheritdoc />
        public void File(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length > 78)
            {
                path = path.Substring(path.Length - 78);
            }

            path = string.Format(CultureInfo.InvariantCulture, @"\subsubsection{{{0}}}", EscapeLatexChars(path));
            this.reportTextWriter.WriteLine(path);
        }

        /// <inheritdoc />
        public void Paragraph(string text)
        {
            this.reportTextWriter.WriteLine(EscapeLatexChars(text));
        }

        /// <inheritdoc />
        public void BeginKeyValueTable()
        {
            this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{ll}");
        }

        /// <inheritdoc />
        public void BeginSummaryTable(bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            this.reportTextWriter.Write(@"\begin{longtable}[l]{|l|r|r|r|r|");

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write("r|");
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write("r|");
            }

            this.reportTextWriter.WriteLine("r|}");
            this.reportTextWriter.WriteLine(@"\hline");

            this.reportTextWriter.Write(
                "\\textbf{{{0}}} & \\textbf{{{1}}} & \\textbf{{{2}}} & \\textbf{{{3}}} & \\textbf{{{4}}} & \\textbf{{{5}}}",
                EscapeLatexChars(ReportResources.Name),
                EscapeLatexChars(ReportResources.Covered),
                EscapeLatexChars(ReportResources.Uncovered),
                EscapeLatexChars(ReportResources.Coverable),
                EscapeLatexChars(ReportResources.Total),
                EscapeLatexChars(ReportResources.Coverage));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                    " & \\textbf{{{0}}}",
                    EscapeLatexChars(ReportResources.BranchCoverage));
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write(
                    " & \\textbf{{{0}}}",
                    EscapeLatexChars(ReportResources.CodeElementCoverageQuota));
            }

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <inheritdoc />
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
            this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{lrrll}");
            this.reportTextWriter.Write(string.Join(" & ", headers.Select(h => @"\textbf{" + EscapeLatexChars(h) + "}")));
            this.reportTextWriter.WriteLine(@"\\");
        }

        /// <inheritdoc />
        public void KeyValueRow(string key, string value)
        {
            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"\textbf{{{0}}} & {1}\\",
                ShortenString(key),
                EscapeLatexChars(value));

            this.reportTextWriter.WriteLine(row);
        }

        /// <inheritdoc />
        public void KeyValueRow(string key, IEnumerable<string> files)
        {
            files = files.Select(f => { return f.Length > 78 ? f.Substring(f.Length - 78) : f; });

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"\textbf{{{0}}} & \begin{{minipage}}[t]{{12cm}}{{{1}}}\end{{minipage}} \\",
                ShortenString(key),
                string.Join(@"\\", files.Select(f => EscapeLatexChars(f))));

            this.reportTextWriter.WriteLine(row);
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
                string columns = "|l|" + string.Join("|", metrics.Skip(i * 5).Take(5).Select(m => "r")) + "|";

                this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{" + columns + "}");
                this.reportTextWriter.WriteLine(@"\hline");
                this.reportTextWriter.Write(@"\textbf{" + EscapeLatexChars(ReportResources.Method) + "} & " + string.Join(" & ", metrics.Skip(i * 5).Take(5).Select(m => @"\textbf{" + EscapeLatexChars(m.Name) + "}")));
                this.reportTextWriter.WriteLine(@"\\");
                this.reportTextWriter.WriteLine(@"\hline");

                foreach (var methodMetric in methodMetrics.OrderBy(c => c.Line))
                {
                    StringBuilder sb = new StringBuilder();
                    int counter = 0;
                    foreach (var metric in metrics.Skip(i * 5).Take(5))
                    {
                        if (counter > 0)
                        {
                            sb.Append(" & ");
                        }

                        var metricValue = methodMetric.Metrics.FirstOrDefault(m => m.Equals(metric));

                        if (metricValue != null)
                        {
                            if (metricValue.Value.HasValue)
                            {
                                sb.Append(metricValue.Value.Value.ToString(CultureInfo.InvariantCulture));

                                if (metricValue.MetricType == MetricType.CoveragePercentual)
                                {
                                    sb.Append("\\%");
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

                        counter++;
                    }

                    string row = string.Format(
                        CultureInfo.InvariantCulture,
                        @"\textbf{{{0}}} & {1}\\",
                        EscapeLatexChars(ShortenString(methodMetric.ShortName, 20)),
                        sb.ToString());

                    this.reportTextWriter.WriteLine(row);
                    this.reportTextWriter.WriteLine(@"\hline");
                }

                this.reportTextWriter.WriteLine(@"\end{longtable}");
            }
        }

        /// <inheritdoc />
        public void LineAnalysis(int fileIndex, LineAnalysis analysis)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException(nameof(analysis));
            }

            string formattedLine = analysis.LineContent
                .Replace(((char)11).ToString(), "  ") // replace tab
                .Replace(((char)9).ToString(), "  ") // replace tab
                .Replace("~", " "); // replace '~' since this used for the \verb command

            formattedLine = ShortenString(formattedLine, 120);
            formattedLine = StringHelper.ReplaceInvalidXmlChars(formattedLine);

            string lineVisitStatus;

            switch (analysis.LineVisitStatus)
            {
                case LineVisitStatus.Covered:
                    lineVisitStatus = "green";
                    break;
                case LineVisitStatus.NotCovered:
                    lineVisitStatus = "red";
                    break;
                case LineVisitStatus.PartiallyCovered:
                    lineVisitStatus = "orange";
                    break;
                default:
                    lineVisitStatus = "gray";
                    break;
            }

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"\cellcolor{{{0}}} & {1} & \verb~{2}~ & & \verb~{3}~\\",
                lineVisitStatus,
                analysis.LineVisitStatus != LineVisitStatus.NotCoverable ? analysis.LineVisits.ToString(CultureInfo.InvariantCulture) : string.Empty,
                analysis.LineNumber,
                formattedLine);

            this.reportTextWriter.WriteLine(row);
        }

        /// <inheritdoc />
        public void FinishTable()
        {
            this.reportTextWriter.WriteLine(@"\end{longtable}");
        }

        /// <inheritdoc />
        public void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots)
        {
            if (riskHotspots == null)
            {
                throw new ArgumentNullException(nameof(riskHotspots));
            }

            var codeQualityMetrics = riskHotspots.First().MethodMetric.Metrics
                .Where(m => m.MetricType == MetricType.CodeQuality)
                .ToArray();

            string columns = "|l|l|l|" + string.Join("|", codeQualityMetrics.Select(m => "r")) + "|";

            this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{" + columns + "}");
            this.reportTextWriter.WriteLine(@"\hline");
            this.reportTextWriter.Write(
                "\\textbf{{{0}}} & \\textbf{{{1}}} & \\textbf{{{2}}}",
                EscapeLatexChars(ReportResources.Assembly2),
                EscapeLatexChars(ReportResources.Class2),
                EscapeLatexChars(ReportResources.Method));

            foreach (var metric in codeQualityMetrics)
            {
                this.reportTextWriter.Write(" & \\textbf{{{0}}}", EscapeLatexChars(metric.Name));
            }

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");

            foreach (var riskHotspot in riskHotspots)
            {
                this.reportTextWriter.Write(
                    "{0} & {1} & {2}",
                    EscapeLatexChars(riskHotspot.Assembly.ShortName),
                    EscapeLatexChars(riskHotspot.Class.DisplayName),
                    EscapeLatexChars(riskHotspot.MethodMetric.ShortName));

                foreach (var statusMetric in riskHotspot.StatusMetrics)
                {
                    if (statusMetric.Exceeded)
                    {
                        this.reportTextWriter.Write(" & \\textcolor{{red}}{{{0}}}", statusMetric.Metric.Value.HasValue ? statusMetric.Metric.Value.Value.ToString(CultureInfo.InvariantCulture) : "-");
                    }
                    else
                    {
                        this.reportTextWriter.Write(" & {0}", statusMetric.Metric.Value.HasValue ? statusMetric.Metric.Value.Value.ToString(CultureInfo.InvariantCulture) : "-");
                    }
                }

                this.reportTextWriter.WriteLine(@"\\");
                this.reportTextWriter.WriteLine(@"\hline");
            }

            this.reportTextWriter.WriteLine(@"\end{longtable}");
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
                @"\textbf{{{0}}} & \textbf{{{1}}} & \textbf{{{2}}} & \textbf{{{3}}} & \textbf{{{4}}} & \textbf{{{5}}}",
                EscapeLatexChars(assembly.Name),
                assembly.CoveredLines,
                assembly.CoverableLines - assembly.CoveredLines,
                assembly.CoverableLines,
                assembly.TotalLines.GetValueOrDefault(),
                assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

            this.reportTextWriter.Write(row);

            if (branchCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" & \textbf{{{0}}}",
                    assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            if (methodCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" & \textbf{{{0}}}",
                    assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <inheritdoc />
        public void SummaryClass(Class @class, bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"{0} & {1} & {2} & {3} & {4} & {5}",
                EscapeLatexChars(@class.DisplayName),
                @class.CoveredLines,
                @class.CoverableLines - @class.CoveredLines,
                @class.CoverableLines,
                @class.TotalLines.GetValueOrDefault(),
                @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

            this.reportTextWriter.Write(row);

            if (branchCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" & {0}",
                    @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            if (methodCoverageAvailable)
            {
                row = string.Format(
                    CultureInfo.InvariantCulture,
                    @" & {0}",
                    @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + @"\%" : string.Empty);

                this.reportTextWriter.Write(row);
            }

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <inheritdoc />
        public void SaveSummaryReport(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, "Summary.tex");

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

                System.IO.File.Delete(Path.Combine(targetDirectory, "tmp.tex"));
            }

            byte[] latexEnd = Encoding.UTF8.GetBytes(LatexEnd);

            combinedReportStream.Write(latexEnd, 0, latexEnd.Length);

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
        /// <returns>The shortened string.</returns>
        private static string ShortenString(string text) => ShortenString(text, 78);

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

        /// <summary>
        /// Escapes the latex chars.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with escaped latex chars.</returns>
        private static string EscapeLatexChars(string text) => text
                .Replace(@"\", @"\textbackslash ")
                .Replace("%", @"\%")
                .Replace("#", @"\#")
                .Replace("_", @"\_")
                .Replace("<", "$<$")
                .Replace(">", "$>$");
    }
}
