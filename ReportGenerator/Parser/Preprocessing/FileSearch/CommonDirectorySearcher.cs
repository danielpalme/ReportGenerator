using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// Searches a list of paths for the longest common directory.
    /// </summary>
    internal static class CommonDirectorySearcher
    {
        /// <summary>
        /// Gets the longest common directory of the given paths.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>The longest common directory of the given paths.</returns>
        internal static string GetCommonDirectory(IEnumerable<string> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (!files.Any())
            {
                return null;
            }

            string commonPrefix = string.Empty;

            char[] firstValueChars = files.First().ToCharArray();

            foreach (var currentChar in firstValueChars)
            {
                string currentPrefix = commonPrefix + currentChar;

                if (files.Any(v => !v.StartsWith(currentPrefix, StringComparison.OrdinalIgnoreCase)))
                {
                    return commonPrefix;
                }
                else
                {
                    commonPrefix = currentPrefix;
                }
            }

            return commonPrefix.Substring(0, commonPrefix.LastIndexOf('\\') + 1);
        }
    }
}
