using System;

namespace Palmmedia.ReportGenerator.Parser.Analysis
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
        /// <param name="value">The value.</param>
        internal Metric(string name, Uri explanationUrl, decimal value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.ExplanationUrl = explanationUrl;
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
        /// Gets the value.
        /// </summary>
        public decimal Value { get; internal set; }
    }
}
