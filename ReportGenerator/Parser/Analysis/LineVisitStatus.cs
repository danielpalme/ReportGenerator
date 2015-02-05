namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Indicates the coverage status of a line in a source file.
    /// </summary>
    public enum LineVisitStatus
    {
        /// <summary>
        /// Line can not be covered.
        /// </summary>
        NotCoverable,

        /// <summary>
        /// Line was not covered.
        /// </summary>
        NotCovered,

        /// <summary>
        /// Line was covered.
        /// </summary>
        Covered
    }
}
