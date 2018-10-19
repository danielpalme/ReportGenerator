namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Global settings.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// Gets or sets the number reports that are parsed and processed in parallel.
        /// </summary>
        public int NumberOfReportsParsedInParallel { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of historic coverage files that get parsed.
        /// </summary>
        public int MaximumNumberOfHistoricCoverageFiles { get; set; } = 100;
    }
}
