using System;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Base class for the parser implementations.
    /// </summary>
    internal abstract class ParserBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserBase" /> class.
        /// </summary>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        protected ParserBase(IFilter assemblyFilter, IFilter classFilter, IFilter fileFilter)
        {
            this.AssemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.ClassFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.FileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Gets or sets a value indicating whether class names are interpreted (false) or not (true).
        /// Interpreted means that the coverage data of nested or compiler generated classes is included in the parent class.
        /// In raw mode the coverage data is reported for each class separately.
        /// </summary>
        public bool RawMode { get; set; }

        /// <summary>
        /// Gets the assembly filter.
        /// </summary>
        protected IFilter AssemblyFilter { get; }

        /// <summary>
        /// Gets the class filter.
        /// </summary>
        protected IFilter ClassFilter { get; }

        /// <summary>
        /// Gets the file filter.
        /// </summary>
        protected IFilter FileFilter { get; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString() => this.GetType().Name.Replace("Parser", string.Empty);
    }
}
