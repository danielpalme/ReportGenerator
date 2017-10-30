using System.Collections.Generic;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Reporting.CodeAnalysis
{
    /// <summary>
    /// Represents a risk hotspot.
    /// </summary>
    public class RiskHotspot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RiskHotspot"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="class">The class.</param>
        /// <param name="methodMetric">The method metric.</param>
        /// <param name="statusMetrics">The metric with status.</param>
        public RiskHotspot(Assembly assembly, Class @class, MethodMetric methodMetric, IEnumerable<MetricStatus> statusMetrics)
        {
            this.Assembly = assembly;
            this.Class = @class;
            this.MethodMetric = methodMetric;
            this.StatusMetrics = statusMetrics;
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the class.
        /// </summary>
        public Class Class { get; }

        /// <summary>
        /// Gets the method metric.
        /// </summary>
        public MethodMetric MethodMetric { get; }

        /// <summary>
        /// Gets the metric with status.
        /// </summary>
        public IEnumerable<MetricStatus> StatusMetrics { get; }
    }
}
