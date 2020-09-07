using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.CodeAnalysis
{
    /// <summary>
    /// Represents the status of a metric.
    /// </summary>
    public class MetricStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricStatus"/> class.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="exceeded">Determines whether the metric's threshold is exceeded.</param>
        public MetricStatus(Metric metric, bool exceeded)
        {
            this.Metric = metric;
            this.Exceeded = exceeded;
        }

        /// <summary>
        /// Gets the metric.
        /// </summary>
        public Metric Metric { get; }

        /// <summary>
        /// Gets a value indicating whether the metric's threshold is exceeded.
        /// </summary>
        public bool Exceeded { get; }
    }
}
