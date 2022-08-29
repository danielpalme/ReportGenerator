
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class EnumerableExtensionsTest
    {
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
