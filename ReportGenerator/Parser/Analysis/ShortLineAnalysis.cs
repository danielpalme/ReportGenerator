namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Coverage information of a line in a source file.
    /// </summary>
    public class ShortLineAnalysis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortLineAnalysis"/> class.
        /// </summary>
        /// <param name="lineVisits">The number of line visits.</param>
        internal ShortLineAnalysis(int lineVisits)
        {
            this.LineVisits = lineVisits;

            if (lineVisits == 0)
            {
                this.LineVisitStatus = LineVisitStatus.NotCovered;
            }
            else if (lineVisits > 0)
            {
                this.LineVisitStatus = LineVisitStatus.Covered;
            }
            else
            {
                this.LineVisitStatus = LineVisitStatus.NotCoverable;
            }
        }

        /// <summary>
        /// Gets the line visit status.
        /// </summary>
        public LineVisitStatus LineVisitStatus { get; private set; }

        /// <summary>
        /// Gets the number of line visits.
        /// </summary>
        public int LineVisits { get; private set; }
    }
}
