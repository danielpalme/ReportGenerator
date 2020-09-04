namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Temporar class to create <see cref="Analysis.CodeElement"/> once all required information is available.
    /// </summary>
    internal class CodeElementBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeElementBase" /> class.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="firstLine">The first line.</param>
        public CodeElementBase(string name, int firstLine)
        {
            this.Name = name;
            this.FirstLine = firstLine;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the first line.
        /// </summary>
        public int FirstLine { get; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
