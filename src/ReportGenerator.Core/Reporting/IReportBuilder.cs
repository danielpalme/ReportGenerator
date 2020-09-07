using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting
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
        /// Gets or sets the report context.
        /// </summary>
        /// <value>
        /// The report context.
        /// </value>
        IReportContext ReportContext { get; set; }

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
