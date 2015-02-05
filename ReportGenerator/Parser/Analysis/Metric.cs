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
        /// <param name="value">The value.</param>
        internal Metric(string name, decimal value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public decimal Value { get; internal set; }
    }
}
