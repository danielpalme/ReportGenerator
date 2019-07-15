using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class JsonSerializerTest
    {
        [Fact]
        public void ToJsonString_Null()
        {
            Assert.Equal("null", JsonSerializer.ToJsonString(null));
        }

        [Fact]
        public void ToJsonString_PrimitiveType()
        {
            Assert.Equal("1", JsonSerializer.ToJsonString(1));
            Assert.Equal("2", JsonSerializer.ToJsonString(2d));
            Assert.Equal("2.45", JsonSerializer.ToJsonString(2.45d));
            Assert.Equal("3.41", JsonSerializer.ToJsonString(3.41m));
            Assert.Equal("4.5", JsonSerializer.ToJsonString(4.5f));
            Assert.Equal("true", JsonSerializer.ToJsonString(true));
            Assert.Equal("false", JsonSerializer.ToJsonString(false));
        }

        [Fact]
        public void ToJsonString_String()
        {
            Assert.Equal("\"Text\"", JsonSerializer.ToJsonString("Text"));
        }

        [Fact]
        public void ToJsonString_Object()
        {
            var obj = new
            {
                Text = "Text",
                Int = 1,
                Double = 2.45d,
                Decimal = 3.41,
                Float = 4.5,
                Boolean = true,
                NestedObject = new { SubText = "Inner" }
            };

            Assert.Equal("{ \"Text\": \"Text\", \"Int\": 1, \"Double\": 2.45, \"Decimal\": 3.41, \"Float\": 4.5, \"Boolean\": true, \"NestedObject\": { \"SubText\": \"Inner\" } }", JsonSerializer.ToJsonString(obj));
        }
    }
}
