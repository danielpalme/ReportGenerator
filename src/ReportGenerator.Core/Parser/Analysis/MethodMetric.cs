using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents the metrics of a method.
    /// </summary>
    public class MethodMetric
    {
        /// <summary>
        /// List of metrics.
        /// </summary>
        private readonly List<Metric> metrics = new List<Metric>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetric"/> class.
        /// </summary>
        /// <param name="fullName">The full method name.</param>
        /// <param name="shortName">The short method name.</param>
        /// <param name="metrics">The metrics.</param>
        public MethodMetric(string fullName, string shortName, IEnumerable<Metric> metrics)
        {
            this.FullName = fullName;
            this.ShortName = shortName;
            this.AddMetrics(metrics);
        }

        /// <summary>
        /// Gets the list of metrics.
        /// </summary>
        public IEnumerable<Metric> Metrics => this.metrics;

        /// <summary>
        /// Gets the full name of the method.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int? Line { get; internal set; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ShortName;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(MethodMetric)))
            {
                return false;
            }
            else
            {
                var methodMetric = (MethodMetric)obj;
                return methodMetric.FullName.Equals(this.FullName) && methodMetric.Line == this.Line;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.FullName.GetHashCode() + this.Line.GetHashCode();

        /// <summary>
        /// Adds the given metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        internal void AddMetric(Metric metric)
        {
            this.metrics.Add(metric);
        }

        /// <summary>
        /// Adds the given metrics.
        /// </summary>
        /// <param name="metrics">The metrics to add.</param>
        internal void AddMetrics(IEnumerable<Metric> metrics)
        {
            this.metrics.AddRange(metrics);
        }

        /// <summary>
        /// Merges the given method metric with the current instance.
        /// </summary>
        /// <param name="methodMetric">The method metric to merge.</param>
        internal void Merge(MethodMetric methodMetric)
        {
            if (methodMetric == null)
            {
                throw new ArgumentNullException(nameof(methodMetric));
            }

            foreach (var metric in methodMetric.metrics)
            {
                var existingMetric = this.metrics.FirstOrDefault(m => m.Name == metric.Name);
                if (existingMetric != null)
                {
                    if (existingMetric.Value.HasValue)
                    {
                        if (metric.Value.HasValue)
                        {
                            if (metric.MergeOrder == MetricMergeOrder.HigherIsBetter)
                            {
                                existingMetric.Value = Math.Max(existingMetric.Value.Value, metric.Value.Value);
                            }
                            else
                            {
                                existingMetric.Value = Math.Min(existingMetric.Value.Value, metric.Value.Value);
                            }
                        }
                    }
                    else
                    {
                        existingMetric.Value = metric.Value;
                    }
                }
                else
                {
                    this.AddMetric(metric);
                }
            }
        }
    }
}
