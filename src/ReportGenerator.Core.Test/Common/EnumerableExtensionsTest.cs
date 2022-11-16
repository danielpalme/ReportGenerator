
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void SafeSum_Int()
        {
            var values = new[] { 1, 2, 3 };
            Assert.Equal(6, values.SafeSum(i => i));

            values = new[] { 10, int.MaxValue };
            Assert.Equal(int.MaxValue, values.SafeSum(i => i));
        }

        [Fact]
        public void SafeSum_Long()
        {
            var values = new[] { 1L, 2L, 3L };
            Assert.Equal(6L, values.SafeSum(i => i));

            values = new[] { 10L, long.MaxValue };
            Assert.Equal(long.MaxValue, values.SafeSum(i => i));
        }

        [Fact]
        public void SafeSum_Decimal()
        {
            var values = new[] { 1m, 2m, 3m };
            Assert.Equal(6m, values.SafeSum(i => i));

            values = new[] { 10m, decimal.MaxValue };
            Assert.Equal(decimal.MaxValue, values.SafeSum(i => i));
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void TakeLast(int numberOfElements, IEnumerable<int> input, List<int> expectedResult)
        {
            var result = input.TakeLast(numberOfElements);

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> Encodings =>
            new List<object[]>
            {
                new object[] { 2, new List<int>(), new List<int>() },
                new object[] { 2, new List<int>() { 1 }, new List<int>() { 1 } },
                new object[] { 2, new List<int>() { 1, 2 }, new List<int>() { 1, 2 } },
                new object[] { 2, new List<int>() { 1, 2, 3 }, new List<int>() { 2, 3 } },
                new object[] { 2, new List<int>() { 1, 2, 3, 4 }, new List<int>() { 3, 4 } },
                new object[] { 3, new List<int>() { 1, 2, 3, 4 }, new List<int>() { 2, 3, 4 } },

                new object[] { 2, System.Linq.Enumerable.Empty<int>(), new List<int>() },
                new object[] { 2, System.Linq.Enumerable.Range(1, 1), new List<int>() { 1 } },
                new object[] { 2, System.Linq.Enumerable.Range(1, 2), new List<int>() { 1, 2 } },
                new object[] { 2, System.Linq.Enumerable.Range(1, 3), new List<int>() { 2, 3 } },
                new object[] { 2, System.Linq.Enumerable.Range(1, 4), new List<int>() { 3, 4 } },
                new object[] { 3, System.Linq.Enumerable.Range(1, 4), new List<int>() { 2, 3, 4 } },
            };
    }
}
