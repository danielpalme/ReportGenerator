using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Finds files and directories by matching their path names against a pattern.
    /// </summary>
    internal class Glob
    {
        /// <summary>
        /// Gets or sets a value indicating the pattern to match file and directory names against.
        /// The pattern can contain the following special characters:
        /// <list type="table">
        /// <item>
        /// <term>?</term>
        /// <description>Matches any single character in a file or directory name.</description>
        /// </item>
        /// <item>
        /// <term>*</term>
        /// <description>Matches zero or more characters in a file or directory name.</description>
        /// </item>
        /// <item>
        /// <term>**</term>
        /// <description>Matches zero or more recursve directories.</description>
        /// </item>
        /// <item>
        /// <term>[...]</term>
        /// <description>Matches a set of characters in a name. Syntax is equivalent to character groups in <see cref="System.Text.RegularExpressions.Regex"/>.</description>
        /// </item>
        /// <item>
        /// <term>{group1,group2,...}</term>
        /// <description>Matches any of the pattern groups. Groups can contain groups and patterns.</description>
        /// </item>
        /// </list>
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets a value indicating an action to be performed when an error occurs during pattern matching.
        /// </summary>
        public Action<string> ErrorLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions that occur during matching should be rethrown. Default is false.
        /// </summary>
        public bool ThrowOnError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether case should be ignored in file and directory names. Default is true.
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only directories should be matched. Default is false.
        /// </summary>
        public bool DirectoriesOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Regex"/> objects should be cached. Default is true.
        /// </summary>
        public bool CacheRegexes { get; set; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public Glob()
        {
            IgnoreCase = true;
            CacheRegexes = true;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="pattern">The pattern to be matched. See <see cref="Pattern"/> for syntax.</param>
        public Glob(string pattern)
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Performs a pattern match.
        /// </summary>
        /// <returns>The matched path names</returns>
        public IEnumerable<string> ExpandNames()
        {
            return Expand(Pattern, DirectoriesOnly).Select(f => f.FullName);
        }

        /// <summary>
        /// Performs a pattern match.
        /// </summary>
        /// <returns>The matched <see cref="FileSystemInfo"/> objects</returns>
        public IEnumerable<FileSystemInfo> Expand()
        {
            return Expand(Pattern, DirectoriesOnly);
        }

        private class RegexOrString
        {
            public Regex Regex { get; set; }
            public string Pattern { get; set; }
            public bool IgnoreCase { get; set; }

            public RegexOrString(string pattern, string rawString, bool ignoreCase, bool compileRegex)
            {
                IgnoreCase = ignoreCase;

                try
                {
                    Regex = new Regex(pattern, RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : 0)
                        | (compileRegex ? RegexOptions.Compiled : 0));
                    Pattern = pattern;
                }
                catch
                {
                    Pattern = rawString;
                }
            }

            public bool IsMatch(string input)
            {
                if (Regex != null) return Regex.IsMatch(input);
                return Pattern.Equals(input, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
        }

        private static ConcurrentDictionary<string, RegexOrString> RegexOrStringCache = new ConcurrentDictionary<string, RegexOrString>();

        private RegexOrString CreateRegexOrString(string pattern)
        {
            if (!CacheRegexes) return new RegexOrString(GlobToRegex(pattern), pattern, IgnoreCase, compileRegex: false);

            if (!RegexOrStringCache.TryGetValue(pattern, out RegexOrString regexOrString))
            {
                regexOrString = new RegexOrString(GlobToRegex(pattern), pattern, IgnoreCase, compileRegex: true);
                RegexOrStringCache[pattern] = regexOrString;
            }

            return regexOrString;
        }

        private static readonly char[] GlobCharacters = "*?[]{}".ToCharArray();

        private IEnumerable<FileSystemInfo> Expand(string path, bool dirOnly)
        {
            if (string.IsNullOrEmpty(path))
            {
                yield break;
            }

            // stop looking if there are no more glob characters in the path.
            // but only if ignoring case because FileSystemInfo.Exists always ignores case.
            if (IgnoreCase && path.IndexOfAny(GlobCharacters) < 0)
            {
                FileSystemInfo fsi = null;
                bool exists = false;

                try
                {
                    fsi = dirOnly ? (FileSystemInfo)new DirectoryInfo(path) : new FileInfo(path);
                    exists = fsi.Exists;
                }
                catch (Exception)
                {
                    if (ThrowOnError) throw;
                }

                if (exists) yield return fsi;
                yield break;
            }

            string parent = null;

            try
            {
                parent = Path.GetDirectoryName(path);
            }
            catch (Exception)
            {
                if (ThrowOnError) throw;
                yield break;
            }

            if (parent == null)
            {
                DirectoryInfo dir = null;

                try
                {
                    dir = new DirectoryInfo(path);
                }
                catch (Exception)
                {
                    if (ThrowOnError) throw;
                }

                if (dir != null) yield return dir;
                yield break;
            }

            if (parent == "")
            {
                try
                {
                    parent = Directory.GetCurrentDirectory();
                }
                catch (Exception)
                {
                    if (ThrowOnError) throw;
                }
            }

            var child = Path.GetFileName(path);

            // handle groups that contain folders
            // child will contain unmatched closing brace
            if (child.Count(c => c == '}') > child.Count(c => c == '{'))
            {
                foreach (var group in Ungroup(path))
                {
                    foreach (var item in Expand(group, dirOnly))
                    {
                        yield return item;
                    }
                }

                yield break;
            }

            if (child == "**")
            {
                foreach (DirectoryInfo dir in Expand(parent, true).DistinctBy(d => d.FullName).Cast<DirectoryInfo>())
                {
                    yield return dir;

                    foreach (var subDir in GetDirectories(dir))
                    {
                        yield return subDir;
                    }
                }

                yield break;
            }

            var childRegexes = Ungroup(child).Select(s => CreateRegexOrString(s)).ToList();

            foreach (DirectoryInfo parentDir in Expand(parent, true).DistinctBy(d => d.FullName).Cast<DirectoryInfo>())
            {
                IEnumerable<FileSystemInfo> fileSystemEntries;

                try
                {
                    fileSystemEntries = dirOnly ? parentDir.EnumerateDirectories() : parentDir.EnumerateFileSystemInfos();
                }
                catch (Exception)
                {
                    if (ThrowOnError) throw;
                    continue;
                }

                foreach (var fileSystemEntry in fileSystemEntries)
                {
                    if (childRegexes.Any(r => r.IsMatch(fileSystemEntry.Name)))
                    {
                        yield return fileSystemEntry;
                    }
                }

                if (childRegexes.Any(r => r.Pattern == @"^\.\.$")) yield return parentDir.Parent ?? parentDir;
                if (childRegexes.Any(r => r.Pattern == @"^\.$")) yield return parentDir;
            }
        }

        private static HashSet<char> RegexSpecialChars = new HashSet<char>(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });

        private static string GlobToRegex(string glob)
        {
            var regex = new StringBuilder();
            var characterClass = false;

            regex.Append("^");

            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']') characterClass = false;
                    regex.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '*':
                        regex.Append(".*");
                        break;
                    case '?':
                        regex.Append(".");
                        break;
                    case '[':
                        characterClass = true;
                        regex.Append(c);
                        break;
                    default:
                        if (RegexSpecialChars.Contains(c)) regex.Append('\\');
                        regex.Append(c);
                        break;
                }
            }

            regex.Append("$");

            return regex.ToString();
        }

        private static IEnumerable<string> Ungroup(string path)
        {
            if (!path.Contains('{'))
            {
                yield return path;
                yield break;
            }

            var level = 0;
            var option = new StringBuilder();
            var prefix = "";
            var postfix = "";
            var options = new List<string>();

            for (int i = 0; i < path.Length; i++)
            {
                var c = path[i];

                switch (c)
                {
                    case '{':
                        level++;
                        if (level == 1)
                        {
                            prefix = option.ToString();
                            option.Clear();
                        }
                        else option.Append(c);
                        break;
                    case ',':
                        if (level == 1)
                        {
                            options.Add(option.ToString());
                            option.Clear();
                        }
                        else option.Append(c);
                        break;
                    case '}':
                        level--;
                        if (level == 0)
                        {
                            options.Add(option.ToString());
                            break;
                        }
                        else option.Append(c);
                        break;
                    default:
                        option.Append(c);
                        break;
                }

                if (level == 0 && c == '}' && (i + 1) < path.Length)
                {
                    postfix = path.Substring(i + 1);
                    break;
                }
            }

            if (level > 0) // invalid grouping
            {
                yield return path;
                yield break;
            }

            var postGroups = Ungroup(postfix);

            foreach (var opt in options.SelectMany(o => Ungroup(o)))
            {
                foreach (var postGroup in postGroups)
                {
                    var s = prefix + opt + postGroup;
                    yield return s;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Pattern;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Pattern.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var g = (Glob)obj;
            return Pattern == g.Pattern;
        }

        private static IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo root)
        {
            IEnumerable<DirectoryInfo> subDirs = null;

            try
            {
                subDirs = root.EnumerateDirectories();
            }
            catch (Exception)
            {
                yield break;
            }

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                yield return dirInfo;

                foreach (var recursiveDir in GetDirectories(dirInfo))
                {
                    yield return recursiveDir;
                }
            }
        }
    }

    internal static class Extensions
    {
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
