using System.Collections.Generic;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Interface that has to be implemented by classes capable of rendering reports.
    /// </summary>
    public interface IReportBuilder
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        string ReportType { get; }

        /// <summary>
        /// Gets or sets the target directory where reports are stored.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        string TargetDirectory { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses);

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        void CreateSummaryReport(SummaryResult summaryResult);
    }
}
