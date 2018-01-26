using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Reporting.Rendering;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates summary report in XML format (no reports for classes are generated).
    /// </summary>
    public class XmlSummaryReportBuilder : ReportBuilderBase
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public override string ReportType => "XmlSummary";

        /// <summary>
        /// Gets a value indicating whether class reports can be generated in parallel.
        /// </summary>
        public override bool SupportsParallelClassReportExecution => true;

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public override void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public override void CreateSummaryReport(SummaryResult summaryResult)
        {
            this.CreateSummaryReport(new XmlRenderer(), summaryResult);
        }
    }
}
