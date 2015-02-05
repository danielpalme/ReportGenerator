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
        protected static string ReplaceInvalidPathChars(string path)
        {
            return Regex.Replace(path, "[^\\w^\\.]", "_");
        }

        /// <summary>
        /// Replaces the invalid chars in the given string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with replaced invalid chars.</returns>
        protected static string ReplaceInvalidXmlChars(string text)
        {
            return Regex.Replace(text, "[^\\w]", string.Empty);
        }
    }
}
