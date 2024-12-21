namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Coverage information for a test method.
    /// </summary>
    internal class CoverageByTrackedMethod
    {
        /// <summary>
        /// Gets or sets an array containing the coverage information by line number.
        /// -1: Not coverable.
        /// 0: Not visited.
        /// >0: Number of visits.
        /// </summary>
        internal int[] Coverage { get; set; }

        /// <summary>
        /// Gets or sets an array containing the line visit status by line number.
        /// </summary>
        internal LineVisitStatus[] LineVisitStatus { get; set; }
    }
}
