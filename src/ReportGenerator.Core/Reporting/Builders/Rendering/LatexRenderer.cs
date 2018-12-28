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
    internal class LatexRenderer : RendererBase, IReportRenderer, IDisposable
    {
        #region Latex Snippets

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

        #endregion

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

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsCharts => false;

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsRiskHotsSpots => true;

        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="title">The title.</param>
        public void BeginSummaryReport(string targetDirectory, string fileName, string title)
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

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void BeginClassReport(string targetDirectory, string assemblyName, string className)
        {
            if (this.classReportTextWriter == null)
            {
                string targetPath = Path.Combine(targetDirectory, "tmp.tex");

                this.classReportStream = new FileStream(targetPath, FileMode.Create);
                this.classReportTextWriter = new StreamWriter(this.classReportStream);
            }

            this.reportTextWriter = this.classReportTextWriter;
            this.reportTextWriter.WriteLine(@"\newpage");

            className = string.Format(CultureInfo.InvariantCulture, @"\section{{{0}}}", EscapeLatexChars(className));
            this.reportTextWriter.WriteLine(className);
        }

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Header(string text)
        {
            text = string.Format(
                CultureInfo.InvariantCulture,
                @"\{0}section{{{1}}}",
                this.reportTextWriter == this.classReportTextWriter ? "sub" : string.Empty,
                EscapeLatexChars(text));
            this.reportTextWriter.WriteLine(text);
        }

        /// <summary>
        /// Adds the test methods to the report.
        /// </summary>
        /// <param name="testMethods">The test methods.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        /// <param name="codeElementsByFileIndex">Code elements by file index.</param>
        public void TestMethods(IEnumerable<TestMethod> testMethods, IEnumerable<FileAnalysis> fileAnalyses, IDictionary<int, IEnumerable<CodeElement>> codeElementsByFileIndex)
        {
        }

        /// <summary>
        /// Adds a file of a class to a report.
        /// </summary>
        /// <param name="path">The path of the file.</param>
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

        /// <summary>
        /// Adds a paragraph to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Paragraph(string text)
        {
            this.reportTextWriter.WriteLine(EscapeLatexChars(text));
        }

        /// <summary>
        /// Adds a table with two columns to the report.
        /// </summary>
        public void BeginKeyValueTable()
        {
            this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{ll}");
        }

        /// <summary>
        /// Start of risk summary table section.
        /// </summary>
        public void BeginSummaryTable()
        {
        }

        /// <summary>
        /// End of risk summary table section.
        /// </summary>
        public void FinishSummaryTable()
        {
        }

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void BeginSummaryTable(bool branchCoverageAvailable)
        {
            this.reportTextWriter.Write(@"\begin{longtable}[l]{|l|r|r|r|r|r|");

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write("r|");
            }

            this.reportTextWriter.WriteLine("}");
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

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable)
        {
        }

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
            this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{lrrll}");
            this.reportTextWriter.Write(string.Join(" & ", headers.Select(h => @"\textbf{" + EscapeLatexChars(h) + "}")));
            this.reportTextWriter.WriteLine(@"\\");
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="value">The text of the second column.</param>
        public void KeyValueRow(string key, string value)
        {
            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"\textbf{{{0}}} & {1}\\",
                ShortenString(key),
                EscapeLatexChars(value));

            this.reportTextWriter.WriteLine(row);
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="files">The files.</param>
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

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        public void MetricsTable(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var methodMetrics = @class.Files.SelectMany(f => f.MethodMetrics);

            this.MetricsTable(methodMetrics);
        }

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="methodMetrics">The method metrics.</param>
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
            if (methodMetrics == null)
            {
                throw new ArgumentNullException(nameof(methodMetrics));
            }

            var firstMethodMetric = methodMetrics.First();
            int numberOfTables = (int)Math.Ceiling((double)firstMethodMetric.Metrics.Count() / 5);

            for (int i = 0; i < numberOfTables; i++)
            {
                string columns = "|l|" + string.Join("|", firstMethodMetric.Metrics.Skip(i * 5).Take(5).Select(m => "r")) + "|";

                this.reportTextWriter.WriteLine(@"\begin{longtable}[l]{" + columns + "}");
                this.reportTextWriter.WriteLine(@"\hline");
                this.reportTextWriter.Write(@"\textbf{" + EscapeLatexChars(ReportResources.Method) + "} & " + string.Join(" & ", firstMethodMetric.Metrics.Skip(i * 5).Take(5).Select(m => @"\textbf{" + EscapeLatexChars(m.Name) + "}")));
                this.reportTextWriter.WriteLine(@"\\");
                this.reportTextWriter.WriteLine(@"\hline");

                foreach (var methodMetric in methodMetrics.OrderBy(c => c.Line))
                {
                    string metrics = string.Join(" & ", methodMetric.Metrics.Skip(i * 5).Take(5).Select(m => string.Format("{0}{1}", m.Value.HasValue ? m.Value.Value.ToString(CultureInfo.InvariantCulture) : "-", m.Value.HasValue && m.MetricType == MetricType.CoveragePercentual ? "\\%" : string.Empty)));

                    string row = string.Format(
                        CultureInfo.InvariantCulture,
                        @"\textbf{{{0}}} & {1}\\",
                        EscapeLatexChars(ShortenString(methodMetric.ShortName, 20)),
                        metrics);

                    this.reportTextWriter.WriteLine(row);
                    this.reportTextWriter.WriteLine(@"\hline");
                }

                this.reportTextWriter.WriteLine(@"\end{longtable}");
            }
        }

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="fileIndex">The index of the file.</param>
        /// <param name="analysis">The line analysis.</param>
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
            formattedLine = XmlRenderer.ReplaceInvalidXmlChars(formattedLine);

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

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        public void FinishTable()
        {
            this.reportTextWriter.WriteLine(@"\end{longtable}");
        }

        /// <summary>
        /// Renderes a chart with the given historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="renderPngFallBackImage">Indicates whether PNG images are rendered as a fallback.</param>
        public void Chart(IEnumerable<HistoricCoverage> historicCoverages, bool renderPngFallBackImage)
        {
        }

        /// <summary>
        /// Start of risk hotspots section.
        /// </summary>
        public void BeginRiskHotspots()
        {
        }

        /// <summary>
        /// End of risk hotspots section.
        /// </summary>
        public void FinishRiskHotspots()
        {
        }

        /// <summary>
        /// Summary of risk hotspots.
        /// </summary>
        /// <param name="riskHotspots">The risk hotspots.</param>
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
                    EscapeLatexChars(riskHotspot.Class.Name),
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

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable)
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

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryClass(Class @class, bool branchCoverageAvailable)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            string row = string.Format(
                CultureInfo.InvariantCulture,
                @"{0} & {1} & {2} & {3} & {4} & {5}",
                EscapeLatexChars(@class.Name),
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

            this.reportTextWriter.WriteLine(@"\\");
            this.reportTextWriter.WriteLine(@"\hline");
        }

        /// <summary>
        /// Adds the footer to the report.
        /// </summary>
        public void AddFooter()
        {
        }

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        public void SaveSummaryReport(string targetDirectory)
        {
            Stream combinedReportStream = null;

            string targetPath = Path.Combine(targetDirectory, "Summary.tex");

            Logger.InfoFormat("  " + Resources.WritingReportFile, targetPath);

            combinedReportStream = new FileStream(targetPath, FileMode.Create);

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
        /// Saves a class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void SaveClassReport(string targetDirectory, string assemblyName, string className)
        {
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
                .Replace("_", @"\_");
    }
}
