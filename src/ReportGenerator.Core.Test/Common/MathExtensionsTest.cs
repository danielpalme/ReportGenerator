using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class MathExtensionsTest
    {
        [Theory]
        [InlineData(1, 1000, 1, 0.1)]
        [InlineData(1, 10000, 1, 0)]
        [InlineData(1, 100000, 1, 0)]

        [InlineData(1, 1000, 2, 0.1)]
        [InlineData(1, 10000, 2, 0.01)]
        [InlineData(1, 100000, 2, 0)]

        [InlineData(1, 1000, 3, 0.1)]
        [InlineData(1, 10000, 3, 0.01)]
        [InlineData(1, 100000, 3, 0.001)]
        public void CalculatePercentage(int number1, int number2, int maximumDecimalPlaces, decimal expected)
        {
            MathExtensions.MaximumDecimalPlaces = maximumDecimalPlaces;
            decimal result = MathExtensions.CalculatePercentage(number1, number2);
            Assert.Equal(expected, result);

            MathExtensions.MaximumDecimalPlaces = 1;
        }
    }
}
