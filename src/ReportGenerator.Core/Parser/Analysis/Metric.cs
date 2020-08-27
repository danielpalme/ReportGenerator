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
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="explanationUrl">The explanation url.</param>
        /// <param name="metricType">The type of the metric.</param>
        /// <param name="value">The value.</param>
        public Metric(string name, Uri explanationUrl, MetricType metricType, decimal? value, MetricMergeOrder mergeOrder)
            : this(name, explanationUrl, metricType, value)
        {
            this.MergeOrder = mergeOrder;
        }

        /// <summary>
        /// Gets the merge order.
        /// </summary>
        public MetricMergeOrder MergeOrder { get; }

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

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(Metric)))
            {
                return false;
            }
            else
            {
                var metric = (Metric)obj;
                return metric.Name.Equals(this.Name);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.Name.GetHashCode();
    }
}
