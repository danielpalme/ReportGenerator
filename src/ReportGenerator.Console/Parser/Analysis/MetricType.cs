namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Type of a <see cref="Metric"/>.
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// Percentual value (e.g. line coverage).
        /// </summary>
        Percentage,

        /// <summary>
        /// Code quality indicator (e.g. cyclomatic complexity).
        /// </summary>
        CodeQuality,

        /// <summary>
        /// A sumable metric (e.g. number of covered/uncovered blocks).
        /// </summary>
        Sumable
    }
}
