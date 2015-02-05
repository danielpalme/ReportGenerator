namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Indicates the method how coverage of a class was measured.
    /// </summary>
    public enum CoverageType
    {
        /// <summary>
        /// Coverage was measured line by line.
        /// </summary>
        LineCoverage,

        /// <summary>
        /// Coverage was measured by methods.
        /// </summary>
        MethodCoverage
    }
}
