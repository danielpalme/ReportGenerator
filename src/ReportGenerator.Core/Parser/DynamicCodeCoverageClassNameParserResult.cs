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
        /// <param name="displayName">The display name.</param>
        /// <param name="include">Indicates whether the class should be included in the report.</param>
        public DynamicCodeCoverageClassNameParserResult(
            string namespaceOfClass,
            string name,
            string displayName,
            bool include)
        {
            this.Namespace = namespaceOfClass;
            this.Name = name;
            this.DisplayName = displayName;
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
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets or sets the generic type.
        /// </summary>
        public string GenericType { get; set; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName => this.Namespace == null ? this.DisplayName : $"{this.Namespace}.{this.DisplayName}";

        /// <summary>
        /// Gets a value indicating whether the class should be included in the report.
        /// </summary>
        public bool Include { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.DisplayName;
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
                    && string.Equals(classNameParserResult.DisplayName, this.DisplayName)
                    && string.Equals(classNameParserResult.Namespace, this.Namespace);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Name.GetHashCode() + this.DisplayName.GetHashCode() + (this.Namespace?.GetHashCode()).GetValueOrDefault();
    }
}
