using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Full coverage information of a line in a source file.
    /// </summary>
    public class LineAnalysis : ShortLineAnalysis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineAnalysis" /> class.
        /// </summary>
        /// <param name="lineVisits">The number of line visits.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="lineCoverageByTestMethod">The line coverage by test method.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="lineContent">Content of the line.</param>
        internal LineAnalysis(int lineVisits, LineVisitStatus lineVisitStatus, IDictionary<TestMethod, ShortLineAnalysis> lineCoverageByTestMethod, int lineNumber, string lineContent)
            : base(lineVisits, lineVisitStatus)
        {
            this.LineCoverageByTestMethod = lineCoverageByTestMethod;
            this.LineNumber = lineNumber;
            this.LineContent = lineContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineAnalysis" /> class.
        /// </summary>
        /// <param name="lineVisits">The number of line visits.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="lineCoverageByTestMethod">The line coverage by test method.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="lineContent">Content of the line.</param>
        /// <param name="coveredBranches">The covered branches.</param>
        /// <param name="totalBranches">The total branches.</param>
        internal LineAnalysis(int lineVisits, LineVisitStatus lineVisitStatus, IDictionary<TestMethod, ShortLineAnalysis> lineCoverageByTestMethod, int lineNumber, string lineContent, int coveredBranches, int totalBranches)
            : this(lineVisits, lineVisitStatus, lineCoverageByTestMethod, lineNumber, lineContent)
        {
            this.CoveredBranches = coveredBranches;
            this.TotalBranches = totalBranches;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the content of the line.
        /// </summary>
        /// <value>
        /// The content of the line.
        /// </value>
        public string LineContent { get; }

        /// <summary>
        /// Gets the line coverage by test method.
        /// </summary>
        /// <value>
        /// The line coverage by test method.
        /// </value>
        public IDictionary<TestMethod, ShortLineAnalysis> LineCoverageByTestMethod { get; }

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches { get; }

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches { get; }
    }
}
