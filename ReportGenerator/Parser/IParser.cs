using System.Collections.Generic;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Parser
{
    /// <summary>
    /// Interface for different parsers.
    /// </summary>
    internal interface IParser
    {
        /// <summary>
        /// Gets the assemblies that have been found in the report.
        /// </summary>
        /// <value>The assemblies.</value>
        IEnumerable<Assembly> Assemblies { get; }
    }
}
