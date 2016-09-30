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

            switch (lineVisits)
            {
                case 0:
                    this.LineVisitStatus = LineVisitStatus.NotCovered;
                    break;
                case 1:
                    this.LineVisitStatus = LineVisitStatus.PartiallyCovered;
                    break;
                case 2:
                    this.LineVisitStatus = LineVisitStatus.Covered;
                    break;
                default:
                    this.LineVisitStatus = LineVisitStatus.NotCoverable;
                    break;
            }
        }

        /// <summary>
        /// Gets the line visit status.
        /// </summary>
        public LineVisitStatus LineVisitStatus { get; }

        /// <summary>
        /// Gets the number of line visits.
        /// </summary>
        public int LineVisits { get; }
    }
}
