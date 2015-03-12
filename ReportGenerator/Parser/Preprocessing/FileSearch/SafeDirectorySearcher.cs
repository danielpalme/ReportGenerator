using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// Searches directories for files and ignores directories that are not accessible.
    /// </summary>
    internal static class SafeDirectorySearcher
    {
        /// <summary>
        /// Returns an enumerable collection of files names that match a search patter in a specified path.
        /// Directories that can not be accessed are ignored.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>The found files.</returns>
        internal static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOption == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOption));
                }

                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
