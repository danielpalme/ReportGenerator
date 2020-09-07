using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Searches files based on file pattern with support for globbing.
    /// </summary>
    internal static class GlobbingFileSearch
    {
        /// <summary>
        /// Gets the files matching the given file pattern..
        /// </summary>
        /// <param name="pattern">The file pattern.</param>
        /// <returns>The files.</returns>
        internal static IEnumerable<string> GetFiles(string pattern)
        {
            return new Glob(pattern).ExpandNames();
        }
    }
}
