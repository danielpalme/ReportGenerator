using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// TeamCity report renderer.
    /// </summary>
    internal class TeamCityRenderer : RendererBase, IReportRenderer
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(TeamCityRenderer));

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsCharts => false;

        /// <summary>
        /// Gets a value indicating whether renderer support rendering risk hotspots.
        /// </summary>
        public bool SupportsRiskHotsSpots => false;

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable)
        {
            var allClasses = assemblies.SelectMany(x => x.Classes).Where(x => x.CoverableLines > 0).ToList();

            WriteLineCoverage(assemblies);
            WriteBranchCoverage(assemblies);
            WriteClassStatistics(allClasses);
            WriteMethodStatistics(allClasses);
        }

        /// <summary>
        /// Writes method statistics.
        /// </summary>
        /// <param name="allClasses">The collection with classes from all assemblies.</param>
        private static void WriteMethodStatistics(IList<Class> allClasses)
        {
            var totalMethods = allClasses.SelectMany(x => x.Files)
                .SelectMany(x => x.CodeElements)
                .Count();
            var coveredMethods = allClasses.SelectMany(x => x.Files)
                .SelectMany(x => x.CoveredCodeElements)
                .Count();
            WriteStatistics('M', coveredMethods, totalMethods);
        }

        /// <summary>
        /// Writes method statistics.
        /// </summary>
        /// <param name="allClasses">The collection with classes from all assemblies.</param>
        private static void WriteClassStatistics(IList<Class> allClasses)
        {
            var totalClasses = allClasses.Count;
            var coveredClasses = allClasses.Count(y => y.CoveredLines > 0);
            WriteStatistics('C', coveredClasses, totalClasses);
        }

        /// <summary>
        /// Write branch statistics.
        /// </summary>
        /// <param name="assemblies">The collection of all assemblies.</param>
        private static void WriteBranchCoverage(IEnumerable<Assembly> assemblies)
        {
            var coveredBranches = SumSafe(assemblies.Select(x => x.CoveredBranches ?? 0));
            var totalBranches = SumSafe(assemblies.Select(x => x.TotalBranches ?? 0));
            WriteStatistics('B', coveredBranches, totalBranches);
        }

        private static void WriteLineCoverage(IEnumerable<Assembly> assemblies)
        {
            var coveredLines = SumSafe(assemblies.Select(x => x.CoveredLines));
            var totalLines = SumSafe(assemblies.Select(x => x.CoverableLines));
            WriteStatistics('S', coveredLines, totalLines);
        }

        private static int SumSafe(IEnumerable<int> numbers)
        {
            return numbers.Concat(new[] { 0 }).Sum();
        }

        /// <summary>
        /// Calculate and write teamcity coverage statistics.
        /// </summary>
        /// <param name="type">S - Line, C- Class, B- Branch, M- Method</param>
        /// <param name="covered">The covered cases</param>
        /// <param name="total">The total number od cases</param>
        private static void WriteStatistics(char type, decimal covered, decimal total)
        {
            if (total != 0)
            {
                WriteStatistic($"CodeCoverage{type}", Math.Round(covered / total * 100, 2));
                WriteStatistic($"CodeCoverageAbs{type}Covered", covered);
                WriteStatistic($"CodeCoverageAbs{type}Total", total);
            }
        }

        /// <summary>
        /// Write teamcity build statistic.
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="value">The value</param>
        private static void WriteStatistic(string name, decimal value)
        {
            Console.WriteLine($"##teamcity[buildStatisticValue key='{name}' value='{value.ToString(CultureInfo.InvariantCulture)}']");
        }

        #region Skiped interface methods

        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="title">The title.</param>
        public void BeginSummaryReport(string targetDirectory, string fileName, string title)
        {
        }

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void BeginClassReport(string targetDirectory, string assemblyName, string className)
        {
        }

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Header(string text)
        {
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
        }

        /// <summary>
        /// Adds a paragraph to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Paragraph(string text)
        {
        }

        /// <summary>
        /// Adds a table with two columns to the report.
        /// </summary>
        public void BeginKeyValueTable()
        {
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
        }

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="value">The text of the second column.</param>
        public void KeyValueRow(string key, string value)
        {
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="files">The files.</param>
        public void KeyValueRow(string key, IEnumerable<string> files)
        {
        }

        /// <summary>
        /// Adds metrics to the report
        /// </summary>
        /// <param name="class">The class.</param>
        public void MetricsTable(Class @class)
        {
        }

        /// <summary>
        /// Adds metrics to the report
        /// </summary>
        /// <param name="methodMetrics">The method metrics.</param>
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
        }

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="fileIndex">The index of the file.</param>
        /// <param name="analysis">The line analysis.</param>
        public void LineAnalysis(int fileIndex, LineAnalysis analysis)
        {
        }

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        public void FinishTable()
        {
        }

        /// <summary>
        /// Renderes a chart with the given historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="renderPngFallBackImage">Indicates whether PNG images are rendered as a fallback</param>
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
        }

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable)
        {
        }

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryClass(Class @class, bool branchCoverageAvailable)
        {
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

        #endregion
    }
}
