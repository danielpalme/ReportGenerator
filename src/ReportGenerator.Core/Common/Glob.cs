using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Finds files and directories by matching their path names against a pattern.
    /// Implementation based on https://github.com/mganss/Glob.cs.
    /// </summary>
    internal class Glob
    {
        private static readonly char[] GlobCharacters = "*?[]{}".ToCharArray();

        private static readonly Dictionary<string, RegexOrString> RegexOrStringCache = new Dictionary<string, RegexOrString>();

        private static readonly HashSet<char> RegexSpecialChars = new HashSet<char>(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });

        /// <summary>
        /// Initializes a new instance of the <see cref="Glob"/> class.
        /// </summary>
        public Glob()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Glob"/> class.
        /// </summary>
        /// <param name="pattern">The pattern to be matched. See <see cref="Pattern"/> for syntax.</param>
        public Glob(string pattern)
        {
            this.Pattern = pattern;
        }

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
        /// Gets or sets a value indicating whether case should be ignored in file and directory names. Default is true.
        /// </summary>
        public bool IgnoreCase { get; set; } = true;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Pattern;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Pattern.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var g = (Glob)obj;
            return this.Pattern == g.Pattern;
        }

        /// <summary>
        /// Performs a pattern match.
        /// </summary>
        /// <returns>The matched path names.</returns>
        public IEnumerable<string> ExpandNames()
        {
            return this.Expand(this.Pattern, false).Select(f => f.FullName);
        }

        /// <summary>
        /// Performs a pattern match.
        /// </summary>
        /// <returns>The matched <see cref="FileSystemInfo"/> objects.</returns>
        public IEnumerable<FileSystemInfo> Expand()
        {
            return this.Expand(this.Pattern, false);
        }

        private RegexOrString CreateRegexOrString(string pattern)
        {
            if (!RegexOrStringCache.TryGetValue(pattern, out RegexOrString regexOrString))
            {
                regexOrString = new RegexOrString(GlobToRegex(pattern), pattern, this.IgnoreCase, compileRegex: true);
                RegexOrStringCache[pattern] = regexOrString;
            }

            return regexOrString;
        }

        private IEnumerable<FileSystemInfo> Expand(string path, bool dirOnly)
        {
            if (string.IsNullOrEmpty(path))
            {
                yield break;
            }

            // stop looking if there are no more glob characters in the path.
            // but only if ignoring case because FileSystemInfo.Exists always ignores case.
            if (this.IgnoreCase && path.IndexOfAny(GlobCharacters) < 0)
            {
                FileSystemInfo fsi = null;
                bool exists = false;

                fsi = dirOnly ? (FileSystemInfo)new DirectoryInfo(path) : new FileInfo(path);
                exists = fsi.Exists;

                if (exists)
                {
                    yield return fsi;
                }

                yield break;
            }

            string parent = Path.GetDirectoryName(path);

            if (parent == null)
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                if (dir != null)
                {
                    yield return dir;
                }

                yield break;
            }

            if (parent == string.Empty)
            {
                parent = Directory.GetCurrentDirectory();
            }

            var child = Path.GetFileName(path);

            // handle groups that contain folders
            // child will contain unmatched closing brace
            if (child.Count(c => c == '}') > child.Count(c => c == '{'))
            {
                foreach (var group in Ungroup(path))
                {
                    foreach (var item in this.Expand(group, dirOnly))
                    {
                        yield return item;
                    }
                }

                yield break;
            }

            if (child == "**")
            {
                foreach (DirectoryInfo dir in this.Expand(parent, true).DistinctBy(d => d.FullName).Cast<DirectoryInfo>())
                {
                    yield return dir;

                    foreach (var subDir in GetDirectories(dir))
                    {
                        yield return subDir;
                    }
                }

                yield break;
            }

            var childRegexes = Ungroup(child).Select(s => this.CreateRegexOrString(s)).ToList();

            foreach (DirectoryInfo parentDir in this.Expand(parent, true).DistinctBy(d => d.FullName).Cast<DirectoryInfo>())
            {
                IEnumerable<FileSystemInfo> fileSystemEntries = dirOnly ? parentDir.EnumerateDirectories() : parentDir.EnumerateFileSystemInfos();

                foreach (var fileSystemEntry in fileSystemEntries)
                {
                    if (childRegexes.Any(r => r.IsMatch(fileSystemEntry.Name)))
                    {
                        yield return fileSystemEntry;
                    }
                }

                if (childRegexes.Any(r => r.Pattern == @"^\.\.$"))
                {
                    yield return parentDir.Parent ?? parentDir;
                }

                if (childRegexes.Any(r => r.Pattern == @"^\.$"))
                {
                    yield return parentDir;
                }
            }
        }

        private static string GlobToRegex(string glob)
        {
            var regex = new StringBuilder();
            var characterClass = false;

            regex.Append("^");

            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']')
                    {
                        characterClass = false;
                    }

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
                        if (RegexSpecialChars.Contains(c))
                        {
                            regex.Append('\\');
                        }

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
            var prefix = string.Empty;
            var postfix = string.Empty;
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
                        else
                        {
                            option.Append(c);
                        }

                        break;
                    case ',':
                        if (level == 1)
                        {
                            options.Add(option.ToString());
                            option.Clear();
                        }
                        else
                        {
                            option.Append(c);
                        }

                        break;
                    case '}':
                        level--;
                        if (level == 0)
                        {
                            options.Add(option.ToString());
                            break;
                        }
                        else
                        {
                            option.Append(c);
                        }

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

            // invalid grouping
            if (level > 0)
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

        private static IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo root)
        {
            IEnumerable<DirectoryInfo> subDirs;

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

        private class RegexOrString
        {
            public RegexOrString(string pattern, string rawString, bool ignoreCase, bool compileRegex)
            {
                this.IgnoreCase = ignoreCase;

                try
                {
                    this.Regex = new Regex(
                        pattern,
                        RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : 0) | (compileRegex ? RegexOptions.Compiled : 0));
                    this.Pattern = pattern;
                }
                catch
                {
                    this.Pattern = rawString;
                }
            }

            public Regex Regex { get; set; }

            public string Pattern { get; set; }

            public bool IgnoreCase { get; set; }

            public bool IsMatch(string input)
            {
                if (this.Regex != null)
                {
                    return this.Regex.IsMatch(input);
                }

                return this.Pattern.Equals(input, this.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
        }
    }
}
