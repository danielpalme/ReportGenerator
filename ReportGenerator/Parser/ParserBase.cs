using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Parser
{
    /// <summary>
    /// Base class for the <see cref="IParser"/> implementations.
    /// </summary>
    internal abstract class ParserBase : IParser
    {
        /// <summary>
        /// The assemblies found in the report.
        /// </summary>
        private ConcurrentBag<Assembly> assemblies = new ConcurrentBag<Assembly>();

        /// <summary>
        /// Gets the assemblies that have been found in the report.
        /// </summary>
        /// <value>The assemblies.</value>
        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                return this.assemblies.OrderBy(a => a.Name);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.GetType().Name;
        }

        /// <summary>
        /// Adds the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        protected internal void AddAssembly(Assembly assembly)
        {
            this.assemblies.Add(assembly);
        }
    }
}
