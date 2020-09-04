using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class StringExtensionsTest
    {
        [Fact]
        public void ParseLargeInteger_CorrectResultReturned()
        {
            Assert.Equal(0, "0".ParseLargeInteger());
            Assert.Equal(100, "100".ParseLargeInteger());
            Assert.Equal(1000, "1000".ParseLargeInteger());
            Assert.Equal(int.MaxValue, "2147483649".ParseLargeInteger());
        }
    }
}
