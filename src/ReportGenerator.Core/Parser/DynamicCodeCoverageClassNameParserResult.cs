namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Result of the <see cref="DynamicCodeCoverageClassNameParser"/>.
    /// </summary>
    internal class DynamicCodeCoverageClassNameParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCodeCoverageClassNameParserResult"/> class.
        /// </summary>
        /// <param name="namespaceOfClass">The namespace.</param>
        /// <param name="name">The name.</param>
        /// <param name="include">Indicates whether the class should be included in the report.</param>
        public DynamicCodeCoverageClassNameParserResult(
            string namespaceOfClass,
            string name,
            bool include)
        {
            this.Namespace = namespaceOfClass;
            this.Name = name;
            this.Include = include;
        }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName => this.Namespace == null ? this.Name : $"{this.Namespace}.{this.Name}";

        /// <summary>
        /// Gets a value indicating whether the class should be included in the report.
        /// </summary>
        public bool Include { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(DynamicCodeCoverageClassNameParserResult)))
            {
                return false;
            }
            else
            {
                var classNameParserResult = (DynamicCodeCoverageClassNameParserResult)obj;
                return string.Equals(classNameParserResult.Name, this.Name)
                    && string.Equals(classNameParserResult.Namespace, this.Namespace);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Name.GetHashCode() + (this.Namespace?.GetHashCode()).GetValueOrDefault();
    }
}
