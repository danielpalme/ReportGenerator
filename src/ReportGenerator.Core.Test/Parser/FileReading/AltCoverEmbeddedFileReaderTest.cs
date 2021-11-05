using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.FileReading
{
    /// <summary>
    /// This is a test class for AltCoverEmbeddedFileReader and is intended
    /// to contain all AltCoverEmbeddedFileReader Unit Tests
    /// </summary>
    public class AltCoverEmbeddedFileReaderTest
    {
        [Fact]
        public void ValidEncodedFile_CorrectContentAndNoError()
        {
            var sut = new AltCoverEmbeddedFileReader("hY7BCoJQEEX3Qf8wS4XoAxIDd21qk62ixWiDCqOJM4+Q6Mta9En9Qs+eGRTS3TyYd++593G7Tydgtd+2olTOI9WmSIzSTjAj7/saY5ORyjwSoTLhdgYR8+m8NqxFzRRqY8g/OGRRKTUVMggh0xFSRhFYG1FMcxqQC/gucfGLezrVJuEiBbH/VQYbLAkuYHcEcB0zxVTWjPrfuELJx00/a70+VtkRszdD+7LhkFuo/4F1CpfgbV6heHB33T6E4DnaB/PKBw5gRz0B");

            string[] lines = sut.LoadFile("DoesNotMatter", out string error);
            Assert.Null(error);
            Assert.True(lines.Length > 0);
        }

        [Fact]
        public void InvalidEncodedFile_InvalidContent_Error()
        {
            var sut = new AltCoverEmbeddedFileReader("xyz");

            string[] lines = sut.LoadFile("DoesNotMatter", out string error);
            Assert.NotNull(error);
            Assert.Null(lines);
        }
    }
}
