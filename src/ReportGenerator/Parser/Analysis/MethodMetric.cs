using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Represents the metrics of a method.
    /// </summary>
    public class MethodMetric
    {
        /// <summary>
        /// Regex to analyze/split a method name.
        /// </summary>
        private const string MethodRegex = @"^.*::(?<MethodName>.+)\((?<Arguments>.*)\)$";

        /// <summary>
        /// List of metrics.
        /// </summary>
        private readonly List<Metric> metrics = new List<Metric>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        internal MethodMetric(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="metrics">The metrics.</param>
        internal MethodMetric(string name, IEnumerable<Metric> metrics)
        {
            this.Name = name;
            this.AddMetrics(metrics);
        }

        /// <summary>
        /// Gets the list of metrics.
        /// </summary>
        public IEnumerable<Metric> Metrics => this.metrics;

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the short name of the method (method arguments are omitted).
        /// </summary>
        /// <value>The short name of the method.</value>
        public string ShortName => Regex.Replace(
                    this.Name,
                    MethodRegex,
                    m => string.Format(CultureInfo.InvariantCulture, "{0}({1})", m.Groups["MethodName"].Value, m.Groups["Arguments"].Value.Length > 0 ? "..." : string.Empty));

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
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
                return methodMetric.Name.Equals(this.Name);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => this.Name.GetHashCode();

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
                    existingMetric.Value = Math.Max(existingMetric.Value, metric.Value);
                }
                else
                {
                    this.AddMetric(metric);
                }
            }
        }
    }
}
