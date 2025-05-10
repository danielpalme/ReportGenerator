namespace Palmmedia.ReportGenerator.Core.CodeAnalysis
{
    /// <summary>
    /// Thresholds of the various metrics.
    /// </summary>
    public class RiskHotspotsAnalysisThresholds
    {
        /// <summary>
        /// Gets or sets the threshold for cylomatic complexity.
        /// </summary>
        public decimal MetricThresholdForCyclomaticComplexity { get; set; } = 15;

        /// <summary>
        /// Gets or sets the threshold for crap score.
        /// </summary>
        public decimal MetricThresholdForCrapScore { get; set; } = 30;

        /// <summary>
        /// Gets or sets the threshold for NPath complexity.
        /// </summary>
        public decimal MetricThresholdForNPathComplexity { get; set; } = 200;

        /// <summary>
        /// Gets or sets the maximum threshold for cylomatic complexity. If any risk hotspot is greater this this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public decimal? MaximumThresholdForCyclomaticComplexity { get; set; }

        /// <summary>
        /// Gets or sets the maximum threshold for crap score. If any risk hotspot is greater this this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public decimal? MaximumThresholdForCrapScore { get; set; }

        /// <summary>
        /// Gets or sets the maximum threshold for NPath complexity. If any risk hotspot is greater this this treshold, ReportGenerator will exit unsuccessfully.
        /// </summary>
        public decimal? MaximumThresholdForNPathComplexity { get; set; }
    }
}