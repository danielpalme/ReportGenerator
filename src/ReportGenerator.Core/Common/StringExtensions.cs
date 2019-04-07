using System.Globalization;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses integers from string.
        /// If parsing fails int.MaxValue is returned.
        /// </summary>
        /// <param name="input">The number as string.</param>
        /// <returns>The parsed number.</returns>
        public static int ParseLargeInteger(this string input)
        {
            if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            else
            {
                return int.MaxValue;
            }
        }
    }
}
