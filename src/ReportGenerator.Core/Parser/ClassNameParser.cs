using System;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Parses class names and extracts generic type information.
    /// </summary>
    internal static class ClassNameParser
    {
        /// <summary>
        /// Regex to clean class names from compiler generated parts.
        /// </summary>
        private static readonly Regex CleanupRegex = new Regex(".<.*>\\w_?_?\\w*\\d*", RegexOptions.Compiled);

        /// <summary>
        /// Regex to analyze if a class name represents a generic class.
        /// </summary>
        private static readonly Regex GenericClassRegex = new Regex("(?<ClassName>.+)(?<GenericTypes><.+>)$", RegexOptions.Compiled);

        /// <summary>
        /// Parses the class name and extracts generic type information.
        /// </summary>
        /// <param name="rawName">The raw/full name.</param>
        /// <param name="rawMode">Indicates whether class names are interpreted (false) or not (true).</param>
        /// <returns>The parser result.</returns>
        public static ClassNameParserResult ParseClassName(string rawName, bool rawMode)
        {
            if (rawName == null)
            {
                throw new ArgumentNullException(nameof(rawName));
            }

            if (rawMode)
            {
                return new ClassNameParserResult(rawName, rawName, rawName, true);
            }

            int nestedClassSeparatorIndex = rawName.IndexOf('/');

            if (nestedClassSeparatorIndex > -1)
            {
                string className = rawName.Substring(0, nestedClassSeparatorIndex);
                return new ClassNameParserResult(className, className, rawName, IncludeClass(className));
            }

            if (rawName.Contains("<"))
            {
                string cleanedClassName = CleanupRegex.Replace(rawName, string.Empty);

                if (cleanedClassName.Equals(rawName))
                {
                    return new ClassNameParserResult(rawName, rawName, rawName, IncludeClass(rawName));
                }

                var match = GenericClassRegex.Match(cleanedClassName);

                if (match.Success)
                {
                    return new ClassNameParserResult(
                        match.Groups["ClassName"].Value,
                        match.Groups["ClassName"].Value + match.Groups["GenericTypes"].Value,
                        rawName,
                        IncludeClass(match.Groups["ClassName"].Value));
                }
                else
                {
                    return new ClassNameParserResult(
                        cleanedClassName,
                        cleanedClassName,
                        rawName,
                        IncludeClass(cleanedClassName));
                }
            }

            return new ClassNameParserResult(rawName, rawName, rawName, IncludeClass(rawName));
        }

        /// <summary>
        /// Determines whether the given class name should be included in the report.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <returns>True if the class should be included; otherwise, false.</returns>
        private static bool IncludeClass(string name)
        {
            return !name.Contains("$");
        }
    }
}
