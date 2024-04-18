using System;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    /// <summary>
    /// This is a test class for FileSearch and is intended
    /// to contain all FileSearch Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class WildCardFileSearchTest
    {
        [Fact]
        public void GetFiles_FilePatternNull_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WildCardFileSearch.GetFiles(null).ToArray());
        }

        [Fact]
        public void GetFiles_FilePatternEmtpy_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WildCardFileSearch.GetFiles(string.Empty).ToArray());
        }

        [Fact]
        public void GetFiles_FilePatternInvalid_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WildCardFileSearch.GetFiles("\t").ToArray());
        }

        [Fact]
        public void GetFiles_OnlyDriveWithoutFilePattern_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WildCardFileSearch.GetFiles("C:\\").ToArray());
        }

        [Fact]
        public void GetFiles_OnlyUNCPathWithoutFilePattern_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WildCardFileSearch.GetFiles("\\test").ToArray());
        }

        [Fact]
        public void GetFiles_EmptyDirectory_NoFilesFound()
        {
            Directory.CreateDirectory("tmp");

            var files = WildCardFileSearch.GetFiles(Path.Combine("tmp", "*")).ToArray();
            Assert.Empty(files);

            Directory.Delete("tmp");
        }

        [Fact]
        public void GetFiles_SingleDirectory_XmlFilesFound()
        {
            var files = WildCardFileSearch.GetFiles(Path.Combine(FileManager.GetCSharpReportDirectory(), "*.xml")).ToArray();
            Assert.Equal(24, files.Length);
        }

        [Fact]
        public void GetFiles_MultiDirectory_AllFilesFound()
        {
            var files = WildCardFileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "*", "*", "*")).ToArray();
            Assert.True(files.Length >= 39);
        }

        [Fact]
        public void GetFiles_MultiDirectory_MatchingFilesFound()
        {
            var files = WildCardFileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "CSharp", "*roject*", "*lyzer*.cs")).ToArray();
            Assert.Single(files);
        }

        [Fact]
        public void GetFiles_RelativePath_DllFound()
        {
            var files = WildCardFileSearch.GetFiles("..\\*\\*.dll").ToArray();
            Assert.Contains(files, f => f.EndsWith(this.GetType().Assembly.GetName().Name + ".dll", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetFiles_UncPath_NoFilesFound()
        {
            var files = WildCardFileSearch.GetFiles(@"\\DoesNotExist\*.xml").ToArray();
            Assert.Empty(files);
        }
    }
}
