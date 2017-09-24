namespace Palmmedia.ReportGenerator.Parser.Preprocessing.CodeAnalysis
{
    /// <summary>
    /// Contains information about the position of a method/property within a source code file.
    /// </summary>
    internal class SourceElementPosition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceElementPosition"/> class.
        /// </summary>
        /// <param name="start">The start line number.</param>
        /// <param name="end">The end line number.</param>
        internal SourceElementPosition(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Gets the start line number.
        /// </summary>
        internal int Start { get; }

        /// <summary>
        /// Gets the end line number
        /// </summary>
        internal int End { get; }
    }
}
