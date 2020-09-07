namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Type of a <see cref="Metric"/>.
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// Percentual value (e.g. line coverage).
        /// </summary>
        CoveragePercentual,

        /// <summary>
        /// A sumable metric (e.g. number of covered/uncovered blocks).
        /// </summary>
        CoverageAbsolute,

        /// <summary>
        /// Code quality indicator (e.g. cyclomatic complexity).
        /// </summary>
        CodeQuality,
    }
}
