using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Coverage information of a source file.
    /// </summary>
    public class FileAnalysis
    {
        /// <summary>
        /// The coverage information of the lines in the source file.
        /// </summary>
        private readonly List<LineAnalysis> lineAnalysis = new List<LineAnalysis>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAnalysis"/> class.
        /// </summary>
        /// <param name="path">The path of the source file.</param>
        internal FileAnalysis(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAnalysis"/> class.
        /// </summary>
        /// <param name="path">The path of the source file.</param>
        /// <param name="error">The error.</param>
        internal FileAnalysis(string path, string error)
            : this(path)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; }

        /// <summary>
        /// Gets the coverage information of the lines in the file.
        /// </summary>
        /// <value>The lines.</value>
        public IEnumerable<LineAnalysis> Lines => this.lineAnalysis;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }

        /// <summary>
        /// Adds the given line analysis to the file analysis.
        /// </summary>
        /// <param name="lineAnalysis">The line analysis.</param>
        internal void AddLineAnalysis(LineAnalysis lineAnalysis)
        {
            this.lineAnalysis.Add(lineAnalysis);
        }
    }
}
