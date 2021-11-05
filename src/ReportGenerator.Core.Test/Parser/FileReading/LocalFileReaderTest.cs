using System.IO;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.FileReading
{
    /// <summary>
    /// This is a test class for LocalFileReader and is intended
    /// to contain all LocalFileReader Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class LocalFileReaderTest
    {
        private static readonly string ExistingFile = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private static readonly string NotExistingFile = Path.Combine(FileManager.GetJavaReportDirectory(), "OpenCover.xml");

        [Fact]
        public void NoSourceDirectories_FileExists_DefaultPathReturned()
        {
            var sut = new LocalFileReader();

            string[] lines = sut.LoadFile(ExistingFile, out string error);
            Assert.Null(error);
            Assert.True(lines.Length > 0);
        }

        [Fact]
        public void NoSourceDirectories_FileNotExists_DefaultPathReturned()
        {
            var sut = new LocalFileReader();

            string[] lines = sut.LoadFile(NotExistingFile, out string error);
            Assert.NotNull(error);
            Assert.Null(lines);
        }

        [Fact]
        public void ExistingSourceDirectory_FileNotExists_MappedPathReturned()
        {
            var sut = new LocalFileReader(new[] { FileManager.GetCSharpReportDirectory() });

            string[] lines = sut.LoadFile(NotExistingFile, out string error);
            Assert.Null(error);
            Assert.True(lines.Length > 0);
        }

        [Fact]
        public void NotExistingSourceDirectory_FileNotExists_DefaultPathReturned()
        {
            var sut = new LocalFileReader(new[] { FileManager.GetCPlusPlusReportDirectory() });

            string[] lines = sut.LoadFile(NotExistingFile, out string error);
            Assert.NotNull(error);
            Assert.Null(lines);
        }

        [Theory]
        [InlineData("/_/OpenCover.xml")]
        [InlineData("/_1/OpenCover.xml")]
        [InlineData("/_2/OpenCover.xml")]
        public void DeterminicticPath_ExistingSourceDirectory_FileExists_MappedPathReturned(string filename)
        {
            var sut = new LocalFileReader(new[] { FileManager.GetCSharpReportDirectory() });

            string[] lines = sut.LoadFile(filename, out string error);
            Assert.Null(error);
            Assert.True(lines.Length > 0);
        }
    }
}
