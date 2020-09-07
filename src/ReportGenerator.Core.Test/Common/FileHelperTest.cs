using System.Collections.Generic;
using System.IO;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    public class FileHelperTest
    {
        [Theory]
        [MemberData(nameof(Encodings))]
        public void GetEncoding(string file, Encoding encoding)
        {
            string path = Path.Combine("Common\\Encodings", file);

            var detectedEncoding = FileHelper.GetEncoding(path);

            Assert.Equal(encoding.CodePage, detectedEncoding.CodePage);
        }

        public static IEnumerable<object[]> Encodings =>
            new List<object[]>
            {
                new object[] { "ansii.txt", Encoding.UTF8 },
                new object[] { "ascii.txt", Encoding.UTF8 },
                new object[] { "latin1.txt", Encoding.GetEncoding("iso-8859-1") },
                new object[] { "unicode_uft16_be.txt", Encoding.BigEndianUnicode },
                new object[] { "unicode_uft16_le.txt", Encoding.Unicode },
                new object[] { "utf8.txt", Encoding.UTF8 },
                new object[] { "utf8_bom.txt", Encoding.UTF8 }
            };
    }
}
