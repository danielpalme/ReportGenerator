using System;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents a metric, which is a key/value pair.
    /// </summary>
    public class Metric
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="explanationUrl">The explanation url.</param>
        /// <param name="metricType">The type of the metric.</param>
        /// <param name="value">The value.</param>
        public Metric(string name, Uri explanationUrl, MetricType metricType, decimal? value)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.ExplanationUrl = explanationUrl;
            this.MetricType = metricType;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the explanation url.
        /// </summary>
        public Uri ExplanationUrl { get; }

        /// <summary>
        /// Gets the metric type.
        /// </summary>
        public MetricType MetricType { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public decimal? Value { get; internal set; }
    }
}
