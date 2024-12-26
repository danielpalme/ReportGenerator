namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Minimum coverage thresholds.
    /// </summary>
    public class MinimumCoverageThresholds
    {
        /// <summary>
        /// Gets or sets minimum line coverage. If line coverage falls below this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public int? LineCoverage { get; set; }

        /// <summary>
        /// Gets or sets minimum branch coverage. If branch coverage falls below this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public int? BranchCoverage { get; set; }

        /// <summary>
        /// Gets or sets minimum method coverage. If method coverage falls below this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public int? MethodCoverage { get; set; }

        /// <summary>
        /// Gets or sets minimum full method coverage. If full method coverage falls below this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public int? FullMethodCoverage { get; set; }
    }
}
