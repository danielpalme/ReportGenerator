namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Coverage information of a line in a source file.
    /// </summary>
    public class ShortLineAnalysis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortLineAnalysis" /> class.
        /// </summary>
        /// <param name="lineVisits">The number of line visits.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        internal ShortLineAnalysis(int lineVisits, LineVisitStatus lineVisitStatus)
        {
            this.LineVisits = lineVisits;
            this.LineVisitStatus = lineVisitStatus;
        }

        /// <summary>
        /// Gets the line visit status.
        /// </summary>
        public LineVisitStatus LineVisitStatus { get; }

        /// <summary>
        /// Gets the number of line visits.
        /// </summary>
        public int LineVisits { get; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.LineVisitStatus}: {this.LineVisits}";
        }
    }
}
