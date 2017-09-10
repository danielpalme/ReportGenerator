using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Common;

namespace Palmmedia.ReportGeneratorTest.Common
{
    /// <summary>
    /// This is a test class for FileSearch and is intended
    /// to contain all FileSearch Unit Tests
    /// </summary>
    [TestClass]
    public class FileSearchTest
    {
        [TestMethod]
        public void GetFiles_FilePatternNull_ArgumentException()
        {
            try
            {
                FileSearch.GetFiles(null).ToArray();
                Assert.Fail("ArgumentException expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void GetFiles_FilePatternEmtpy_ArgumentException()
        {
            try
            {
                FileSearch.GetFiles(string.Empty).ToArray();
                Assert.Fail("ArgumentException expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void GetFiles_FilePatternInvalid_ArgumentException()
        {
            try
            {
                FileSearch.GetFiles("\"").ToArray();
                Assert.Fail("ArgumentException expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void GetFiles_OnlyDriveWithoutFilePattern_ArgumentException()
        {
            try
            {
                FileSearch.GetFiles("C:\\").ToArray();
                Assert.Fail("ArgumentException expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void GetFiles_OnlyUNCPathWithoutFilePattern_ArgumentException()
        {
            try
            {
                FileSearch.GetFiles("\\test").ToArray();
                Assert.Fail("ArgumentException expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void GetFiles_EmptyDirectory_NoFilesFound()
        {
            Directory.CreateDirectory("tmp");

            var files = FileSearch.GetFiles(Path.Combine("tmp", "*")).ToArray();
            Assert.AreEqual(0, files.Length);

            Directory.Delete("tmp");
        }

        [TestMethod]
        public void GetFiles_SingleDirectory_XmlFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetCSharpReportDirectory(), "*.xml")).ToArray();
            Assert.AreEqual(16, files.Length);
        }

        [TestMethod]
        public void GetFiles_MultiDirectory_AllFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "*", "*", "*")).ToArray();
            Assert.IsTrue(files.Length >= 39);
        }

        [TestMethod]
        public void GetFiles_MultiDirectory_MatchingFilesFound()
        {
            var files = FileSearch.GetFiles(Path.Combine(FileManager.GetFilesDirectory(), "CSharp", "*roject*", "*lyzer*.cs")).ToArray();
            Assert.AreEqual(1, files.Length);
        }

        [TestMethod]
        public void GetFiles_RelativePath_DllFound()
        {
            var files = FileSearch.GetFiles("..\\*\\*.dll").ToArray();
            Assert.IsTrue(files.Any(f => f.EndsWith(this.GetType().Assembly.GetName().Name + ".dll", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void GetFiles_UncPath_NoFilesFound()
        {
            var files = FileSearch.GetFiles(@"\\DoesNotExist\*.xml").ToArray();
            Assert.AreEqual(0, files.Length);
        }
    }
}
