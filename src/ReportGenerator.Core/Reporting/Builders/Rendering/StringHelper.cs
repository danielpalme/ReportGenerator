using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Helper methods for <see cref="string"/>.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Replaces the invalid chars in the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The path with replaced invalid chars.</returns>
        public static string ReplaceInvalidPathChars(string path) => Regex.Replace(path, "[^\\w^\\.]", "_");

        /// <summary>
        /// Replaces all non letter chars in the given string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with replaced invalid chars.</returns>
        public static string ReplaceNonLetterChars(string text) => Regex.Replace(text, "[^\\w]", string.Empty);

        /// <summary>
        /// Replaces the invalid XML chars in the given string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with replaced invalid chars.</returns>
        public static string ReplaceInvalidXmlChars(string text) => Regex.Replace(text, @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", string.Empty);
    }
}
