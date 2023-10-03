using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// HTML report renderer.
    /// </summary>
    public interface IHtmlRenderer
    {
        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="title">The title.</param>
        void BeginSummaryReport(string targetDirectory, string fileName, string title);

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="classDisplayName">Display name of the class.</param>
        /// <param name="additionalTitle">Additional title.</param>
        void BeginClassReport(string targetDirectory, Assembly assembly, string className, string classDisplayName, string additionalTitle);

        /// <summary>
        /// Add cards to report.
        /// </summary>
        /// <param name="cards">The cards.</param>
        void Cards(IEnumerable<Card> cards);

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void Header(string text);

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void HeaderWithGithubLinks(string text);

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void HeaderWithBackLink(string text);

        /// <summary>
        /// Adds the test methods to the report.
        /// </summary>
        /// <param name="testMethods">The test methods.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        /// <param name="codeElementsByFileIndex">Code elements by file index.</param>
        void TestMethods(IEnumerable<TestMethod> testMethods, IEnumerable<FileAnalysis> fileAnalyses, IDictionary<int, IEnumerable<CodeElement>> codeElementsByFileIndex);

        /// <summary>
        /// Adds a file of a class to a report.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        void File(string path);

        /// <summary>
        /// Adds a paragraph to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void Paragraph(string text);

        /// <summary>
        /// Start of risk summary table section.
        /// </summary>
        void BeginSummaryTable();

        /// <summary>
        /// End of risk summary table section.
        /// </summary>
        void FinishSummaryTable();

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void BeginSummaryTable(bool branchCoverageAvailable, bool methodCoverageAvailable);

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable, bool methodCoverageAvailable);

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        void BeginLineAnalysisTable(IEnumerable<string> headers);

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        void MetricsTable(Class @class);

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="methodMetrics">The method metrics.</param>
        void MetricsTable(IEnumerable<MethodMetric> methodMetrics);

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="fileIndex">The index of the file.</param>
        /// <param name="analysis">The line analysis.</param>
        void LineAnalysis(int fileIndex, LineAnalysis analysis);

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        void FinishTable();

        /// <summary>
        /// Renderes a chart with the given historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void Chart(IEnumerable<HistoricCoverage> historicCoverages, bool methodCoverageAvailable);

        /// <summary>
        /// Start of risk hotspots section.
        /// </summary>
        void BeginRiskHotspots();

        /// <summary>
        /// End of risk hotspots section.
        /// </summary>
        void FinishRiskHotspots();

        /// <summary>
        /// Summary of risk hotspots.
        /// </summary>
        /// <param name="riskHotspots">The risk hotspots.</param>
        void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots);

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable, bool methodCoverageAvailable);

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void SummaryClass(Class @class, bool branchCoverageAvailable, bool methodCoverageAvailable);

        /// <summary>
        /// Adds the footer to the report.
        /// </summary>
        void AddFooter();

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        void SaveSummaryReport(string targetDirectory);

        /// <summary>
        /// Saves a class report.
        /// </summary><param name="targetDirectory">The target directory.</param>
        /// <param name="className">Name of the class.</param>
        void SaveClassReport(string targetDirectory, string className);
    }
}
