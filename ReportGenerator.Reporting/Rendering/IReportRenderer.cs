using System.Collections.Generic;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Reporting.Rendering
{
    /// <summary>
    /// Interface for report renderers.
    /// </summary>
    public interface IReportRenderer
    {
        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="title">The title.</param>
        void BeginSummaryReport(string targetDirectory, string title);

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        void BeginClassReport(string targetDirectory, string assemblyName, string className);

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        void SaveSummaryReport(string targetDirectory);

        /// <summary>
        /// Saves a class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        void SaveClassReport(string targetDirectory, string assemblyName, string className);

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        void Header(string text);

        /// <summary>
        /// Adds the test methods to the report.
        /// </summary>
        /// <param name="testMethods">The test methods.</param>
        void TestMethods(IEnumerable<TestMethod> testMethods);

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
        void BeginSummaryTable();

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        void CustomSummary(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Adds a metrics table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        void BeginMetricsTable(IEnumerable<string> headers);

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
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        void SummaryAssembly(Assembly assembly);

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        void SummaryClass(Class @class);

        /// <summary>
        /// Adds the given metric values to the report.
        /// </summary>
        /// <param name="metric">The metric.</param>
        void MetricsRow(MethodMetric metric);

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="analysis">The line analysis.</param>
        void LineAnalysis(LineAnalysis analysis);

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        void FinishTable();

        /// <summary>
        /// Charts the specified historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        void Chart(IEnumerable<HistoricCoverage> historicCoverages);
    }
}
