using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Latex report renderer.
    /// </summary>
    public interface ILatexRenderer
    {
        /// <summary>
        /// Begins the summary report.
        /// </summary>
        void BeginSummaryReport();

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="classDisplayName">Display name of the class.</param>
        void BeginClassReport(string targetDirectory, string classDisplayName);

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void Header(string text);

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
        /// Adds a table with two columns to the report.
        /// </summary>
        void BeginKeyValueTable();

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        void BeginSummaryTable(bool branchCoverageAvailable, bool methodCoverageAvailable);

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        void BeginLineAnalysisTable(IEnumerable<string> headers);

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="value">The text of the second column.</param>
        void KeyValueRow(string key, string value);

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="files">The files.</param>
        void KeyValueRow(string key, IEnumerable<string> files);

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
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        void SaveSummaryReport(string targetDirectory);
    }
}
