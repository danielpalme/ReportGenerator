using System;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Common
{
    /// <summary>
    /// This is a test class for FileSearch and is intended
    /// to contain all FileSearch Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class FileSearchTest
    {
        [Fact]
        public void GetFiles_FilePatternNull_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => FileSearch.GetFiles(null).ToArray());
        }

        [Fact]
        public void GetFiles_FilePatternEmtpy_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => FileSearch.GetFiles(string.Empty).ToArray());
        }

        [Fact]
        public void GetFiles_FilePatternInvalid_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => FileSearch.GetFiles("\t").ToArray());
        }

        [Fact]
        public void GetFiles_OnlyDriveWithoutFilePattern_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => FileSearch.GetFiles("C:\\").ToArray());
        }

        [Fact]
        public void GetFiles_OnlyUNCPathWithoutFilePattern_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => FileSearch.GetFiles("\\test").ToArray());
        }

        [Fact]
        public void GetFiles_EmptyDirectory_NoFilesFound()
        {
            Directory.CreateDirectory("tmp");

            var files = FileSearch.GetFiles(Path.Combine("tmp", "*")).ToArray();
            Assert.Empty(files);

            Directory.Delete("tmp");
        }

        [Fact]
        public void GetFiles_SingleDirectory_XmlFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetCSharpReportDirectory(), "*.xml")).ToArray();
            Assert.Equal(16, files.Length);
        }

        [Fact]
        public void GetFiles_MultiDirectory_AllFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "*", "*", "*")).ToArray();
            Assert.True(files.Length >= 39);
        }

        [Fact]
        public void GetFiles_MultiDirectory_MatchingFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "CSharp", "*roject*", "*lyzer*.cs")).ToArray();
            Assert.Single(files);
        }

        [Fact]
        public void GetFiles_RelativePath_DllFound()
        {
            var files = FileSearch.GetFiles("..\\*\\*.dll").ToArray();
            Assert.Contains(files, f => f.EndsWith(this.GetType().Assembly.GetName().Name + ".dll", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetFiles_UncPath_NoFilesFound()
        {
            var files = FileSearch.GetFiles(@"\\DoesNotExist\*.xml").ToArray();
            Assert.Empty(files);
        }
    }
}
