using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Reporting.Rendering
{
    /// <summary>
    /// Base class for the <see cref="IReportRenderer"/> implementations.
    /// </summary>
    public abstract class RendererBase
    {
        /// <summary>
        /// Replaces the invalid chars in the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The path with replaced invalid chars.</returns>
        protected static string ReplaceInvalidPathChars(string path) => Regex.Replace(path, "[^\\w^\\.]", "_");

        /// <summary>
        /// Replaces all non letter chars in the given string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with replaced invalid chars.</returns>
        protected static string ReplaceNonLetterChars(string text) => Regex.Replace(text, "[^\\w]", string.Empty);

        /// <summary>
        /// Replaces the invalid XML chars in the given string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with replaced invalid chars.</returns>
        protected static string ReplaceInvalidXmlChars(string text) => Regex.Replace(text, @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", string.Empty);
    }
}
