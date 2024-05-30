namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Result of the <see cref="ClassNameParser"/>.
    /// </summary>
    internal class ClassNameParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassNameParserResult"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="rawName">The raw/full name.</param>
        /// <param name="include">Indicates whether the class should be included in the report.</param>
        public ClassNameParserResult(
            string name,
            string displayName,
            string rawName,
            bool include)
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.RawName = rawName;
            this.Include = include;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the raw/full name.
        /// </summary>
        public string RawName { get; }

        /// <summary>
        /// Gets a value indicating whether the class should be included in the report.
        /// </summary>
        public bool Include { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.RawName;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(ClassNameParserResult)))
            {
                return false;
            }
            else
            {
                var classNameParserResult = (ClassNameParserResult)obj;
                return classNameParserResult.Name.Equals(this.Name)
                    && classNameParserResult.DisplayName.Equals(this.DisplayName);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Name.GetHashCode() + this.DisplayName.GetHashCode();
    }
}
