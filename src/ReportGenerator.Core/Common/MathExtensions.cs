using System;
using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Math extensions.
    /// </summary>
    internal static class MathExtensions
    {
        private static int maximumDecimalPlaces = 1;

        private static double factor = 1000;

        private static int divisor = 10;

        /// <summary>
        /// Gets or sets the maximum decimal places for coverage quotas / percentages.
        /// </summary>
        public static int MaximumDecimalPlaces
        {
            get
            {
                return maximumDecimalPlaces;
            }

            set
            {
                maximumDecimalPlaces = Math.Min(8, Math.Max(0, value));
                factor = 100 * Math.Pow(10, maximumDecimalPlaces);
                divisor = (int)Math.Pow(10, maximumDecimalPlaces);
            }
        }

        /// <summary>
        /// Creates a <see cref="HashSet&lt;T&gt;"/> from an <see cref="IEnumerable&lt;T&gt;"/>.
        /// </summary>
        /// <param name="number1">The first number.</param>
        /// <param name="number2">The second number.</param>
        /// <returns>The percentage.</returns>
        internal static decimal CalculatePercentage(int number1, int number2)
        {
            if (number2 == 0)
            {
                throw new ArgumentException("Number must not be 0", nameof(number2));
            }

            return (decimal)Math.Truncate(factor * (double)number1 / (double)number2) / divisor;
        }
    }
}
