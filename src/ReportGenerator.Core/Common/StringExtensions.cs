using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        /// <summary>
        /// Splits the string at the specified separator, but ensures that globs are not split.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="separators">List of separators.</param>
        /// <returns>The parts.</returns>
        public static string[] SplitThatEnsuresGlobsAreSafe(this string input, params char[] separators)
        {
            if (separators == null || separators.Length == 0)
            {
                return new string[] { input };
            }

            var parts = new List<string>();
            var braceCount = 0;
            var start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                {
                    braceCount++;
                }
                else if (input[i] == '}')
                {
                    braceCount--;
                }

                if (braceCount > 0 && input.IndexOf('}', i + 1) == -1)
                {
                    braceCount = 0;
                }

                if (separators.Contains(input[i]) && braceCount == 0)
                {
                    parts.Add(input.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            if (start < input.Length)
            {
                parts.Add(input.Substring(start).Trim());
            }

            return parts.ToArray();
        }
    }
}
