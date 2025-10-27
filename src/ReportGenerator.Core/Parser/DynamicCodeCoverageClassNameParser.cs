using System;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Parses class names and extracts generic type information.
    /// </summary>
    internal static class DynamicCodeCoverageClassNameParser
    {
        /// <summary>
        /// Regex to clean class names from compiler generated parts.
        /// </summary>
        private static readonly Regex CleanupRegex = new Regex("\\.<>\\w_?_?\\w*\\d*<.*>|\\.<.*>\\w_?_?\\w*\\d*", RegexOptions.Compiled);

        /// <summary>
        /// Regex to analyze if a class name represents a generic class.
        /// </summary>
        private static readonly Regex GenericClassRegex = new Regex("(?<ClassName>.+)(?<GenericTypes><.+>)$", RegexOptions.Compiled);

        /// <summary>
        /// Parses the class name and extracts generic type information.
        /// </summary>
        /// <param name="rawName">The raw/full name.</param>
        /// <param name="namespaceOfClass">The namespace.</param>
        /// <returns>The parser result.</returns>
        public static DynamicCodeCoverageClassNameParserResult ParseClassName(
            string rawName,
            string namespaceOfClass)
        {
            if (rawName == null)
            {
                throw new ArgumentNullException(nameof(rawName));
            }

            string cleanedClassName = CleanupRegex.Replace(rawName, string.Empty);

            int nestedClassSeparatorIndex = cleanedClassName.IndexOf('.');

            if (nestedClassSeparatorIndex > -1)
            {
                string className = cleanedClassName.Substring(0, nestedClassSeparatorIndex);
                return new DynamicCodeCoverageClassNameParserResult(namespaceOfClass, className, className, IncludeClass(className));
            }

            if (cleanedClassName.Contains("<"))
            {
                var match = GenericClassRegex.Match(cleanedClassName);

                if (match.Success)
                {
                    return new DynamicCodeCoverageClassNameParserResult(
                        namespaceOfClass,
                        match.Groups["ClassName"].Value,
                        match.Groups["ClassName"].Value + match.Groups["GenericTypes"].Value,
                        IncludeClass(match.Groups["ClassName"].Value))
                    {
                        GenericType = match.Groups["GenericTypes"].Value
                    };
                }
                else
                {
                    return new DynamicCodeCoverageClassNameParserResult(
                        namespaceOfClass,
                        cleanedClassName,
                        cleanedClassName,
                        IncludeClass(cleanedClassName));
                }
            }

            return new DynamicCodeCoverageClassNameParserResult(namespaceOfClass, cleanedClassName, cleanedClassName, IncludeClass(cleanedClassName));
        }

        /// <summary>
        /// Determines whether the given class name should be included in the report.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <returns>True if the class should be included; otherwise, false.</returns>
        private static bool IncludeClass(string name)
        {
            return !name.Contains("<>")
                && !name.StartsWith("$", StringComparison.OrdinalIgnoreCase);
        }
    }
}
