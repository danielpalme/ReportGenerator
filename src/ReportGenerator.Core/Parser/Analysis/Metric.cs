using System;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents a metric, which is a key/value pair.
    /// </summary>
    public class Metric
    {
        /// <summary>
        /// The cyclomatic complexity URI.
        /// </summary>
        private static readonly Uri CyclomaticComplexityUri = new Uri("https://en.wikipedia.org/wiki/Cyclomatic_complexity");

        /// <summary>
        /// The code coverage URI.
        /// </summary>
        private static readonly Uri CodeCoverageUri = new Uri("https://en.wikipedia.org/wiki/Code_coverage");

        /// <summary>
        /// The n path complexity URI.
        /// </summary>
        private static readonly Uri NPathComplexityUri = new Uri("https://modess.io/npath-complexity-cyclomatic-complexity-explained");

        /// <summary>
        /// The crap score URI.
        /// </summary>
        private static readonly Uri CrapScoreUri = new Uri("https://googletesting.blogspot.de/2011/02/this-code-is-crap.html");

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="explanationUrl">The explanation url.</param>
        /// <param name="metricType">The type of the metric.</param>
        /// <param name="value">The value.</param>
        public Metric(string name, Uri explanationUrl, MetricType metricType, decimal? value)
            : this(name, name, explanationUrl, metricType, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="abbreviation">The abbreviation.</param>
        /// <param name="explanationUrl">The explanation url.</param>
        /// <param name="metricType">The type of the metric.</param>
        /// <param name="value">The value.</param>
        public Metric(string name, string abbreviation, Uri explanationUrl, MetricType metricType, decimal? value)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Abbreviation = abbreviation.ToLowerInvariant().Replace(" ", string.Empty);
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
        /// <param name="mergeOrder">The merge order.</param>
        public Metric(string name, Uri explanationUrl, MetricType metricType, decimal? value, MetricMergeOrder mergeOrder)
            : this(name, name, explanationUrl, metricType, value)
        {
            this.MergeOrder = mergeOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="abbreviation">The abbreviation.</param>
        /// <param name="explanationUrl">The explanation url.</param>
        /// <param name="metricType">The type of the metric.</param>
        /// <param name="value">The value.</param>
        /// <param name="mergeOrder">The merge order.</param>
        public Metric(string name, string abbreviation, Uri explanationUrl, MetricType metricType, decimal? value, MetricMergeOrder mergeOrder)
            : this(name, abbreviation, explanationUrl, metricType, value)
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
        /// Gets the abbreviation.
        /// </summary>
        public string Abbreviation { get; }

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
        /// Initializes a new instance of the <see cref="Metric"/> class which represents line coverage.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric Coverage(decimal? value)
        {
            return new Metric(
                ReportResources.Coverage,
                "cov",
                CodeCoverageUri,
                MetricType.CoveragePercentual,
                value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents branch coverage.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric BranchCoverage(decimal? value)
        {
            return new Metric(
                ReportResources.BranchCoverage,
                "bcov",
                CodeCoverageUri,
                MetricType.CoveragePercentual,
                value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents sequence coverage.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric SequenceCoverage(decimal? value)
        {
            return new Metric(
                ReportResources.SequenceCoverage,
                "seq",
                CodeCoverageUri,
                MetricType.CoveragePercentual,
                value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents blocks covered.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric BlocksCovered(decimal? value)
        {
            return new Metric(
                ReportResources.BlocksCovered,
                "cb",
                CodeCoverageUri,
                MetricType.CoverageAbsolute,
                value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents blocks not covered.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric BlocksNotCovered(decimal? value)
        {
            return new Metric(
                ReportResources.BlocksNotCovered,
                "ub",
                CodeCoverageUri,
                MetricType.CoverageAbsolute,
                value,
                MetricMergeOrder.LowerIsBetter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents cyclomatic complexity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric CyclomaticComplexity(decimal? value)
        {
            return new Metric(
                ReportResources.CyclomaticComplexity,
                "cc",
                CyclomaticComplexityUri,
                MetricType.CodeQuality,
                value,
                MetricMergeOrder.LowerIsBetter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents NPath complexity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric NPathComplexity(decimal? value)
        {
            return new Metric(
                ReportResources.NPathComplexity,
                "npth",
                NPathComplexityUri,
                MetricType.CodeQuality,
                value,
                MetricMergeOrder.LowerIsBetter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class which represents crap score.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The metric.</returns>
        public static Metric CrapScore(decimal? value)
        {
            return new Metric(
                ReportResources.CrapScore,
                "crp",
                CrapScoreUri,
                MetricType.CodeQuality,
                value,
                MetricMergeOrder.LowerIsBetter);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Name}: {this.Value}";
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
