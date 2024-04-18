using System;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Common
{
    /// <summary>
    /// This is a test class for GlobbingFileSearch and is intended
    /// to contain all GlobbingFileSearch Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class GlobbingFileSearchTest
    {
        [Fact]
        public void GetFiles_EmptyDirectory_NoFilesFound()
        {
            Directory.CreateDirectory("tmp");

            var files = GlobbingFileSearch.GetFiles(Path.Combine("tmp", "*")).ToArray();
            Assert.Empty(files);

            Directory.Delete("tmp");
        }

        [Fact]
        public void GetFiles_SingleDirectory_XmlFilesFound()
        {
            var files = GlobbingFileSearch.GetFiles(Path.Combine(FileManager.GetCSharpReportDirectory(), "*.xml")).ToArray();
            Assert.Equal(24, files.Length);
        }

        [Fact]
        public void GetFiles_MultiDirectory_AllFilesFound()
        {
            var files = GlobbingFileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "*", "*", "*")).ToArray();
            Assert.True(files.Length >= 39);
        }

        [Fact]
        public void GetFiles_MultiDirectory_MatchingFilesFound()
        {
            var files = GlobbingFileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "CSharp", "*roject*", "*lyzer*.cs")).ToArray();
            Assert.Single(files);
        }

        [Fact]
        public void GetFiles_RelativePath_DllFound()
        {
            var files = GlobbingFileSearch.GetFiles("..\\*\\*.dll").ToArray();
            Assert.Contains(files, f => f.EndsWith(this.GetType().Assembly.GetName().Name + ".dll", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetFiles_UncPath_NoFilesFound()
        {
            var files = GlobbingFileSearch.GetFiles(@"\\UncPath\DoesNotExist\*.xml").ToArray();
            Assert.Empty(files);
        }
    }
}
