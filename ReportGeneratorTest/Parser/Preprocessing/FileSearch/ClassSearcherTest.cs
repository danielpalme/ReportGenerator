using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// This is a test class for ClassSearcher and is intended
    /// to contain all ClassSearcher Unit Tests
    /// </summary>
    [TestClass]
    public class ClassSearcherTest
    {
        private static ClassSearcher classSearcher;

        #region Additional test attributes

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            classSearcher = new ClassSearcher("C:\\temp");
        }

        #endregion

        /// <summary>
        /// A test for GetFilesOfClass
        /// </summary>
        [TestMethod]
        public void GetFilesOfClass_PartialClassWith2Files_2FilesFound()
        {
            var files = classSearcher.GetFilesOfClass("Test.PartialClass");

            Assert.IsNotNull(files, "Files must not be null.");
            Assert.IsTrue(files.Contains("C:\\temp\\PartialClass.cs"), "Files does not contain expected file");
            Assert.IsTrue(files.Contains("C:\\temp\\PartialClass2.cs"), "Files does not contain expected file");
        }

        /// <summary>
        /// A test for GetFilesOfClass
        /// </summary>
        [TestMethod]
        public void GetFilesOfClass_NestedClass_1FileFound()
        {
            var files = classSearcher.GetFilesOfClass("Test.TestClassNestedClass");

            Assert.IsNotNull(files, "Files must not be null.");
            Assert.IsTrue(files.Contains("C:\\temp\\TestClass.cs"), "Files does not contain expected file");
        }

        /// <summary>
        /// A test for GetFilesOfClass
        /// </summary>
        [TestMethod]
        public void GetFilesOfClass_NotExistingClass_0FilesFound()
        {
            var files = classSearcher.GetFilesOfClass("Test.Test123");

            Assert.IsNotNull(files, "Files must not be null.");
            Assert.IsFalse(files.Any());
        }
    }
}
