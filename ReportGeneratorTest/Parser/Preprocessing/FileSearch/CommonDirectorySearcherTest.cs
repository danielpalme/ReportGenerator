using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// This is a test class for CommonDirectorySearcher and is intended
    /// to contain all CommonDirectorySearcher Unit Tests
    /// </summary>
    [TestClass]
    public class CommonDirectorySearcherTest
    {
        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCommonDirectory_PassNull_ArgumentNullExceptionIsThrown()
        {
            CommonDirectorySearcher.GetCommonDirectory(null);
        }

        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        public void GetCommonDirectory_EmptyArray_Null()
        {
            Assert.IsNull(CommonDirectorySearcher.GetCommonDirectory(new string[] { }));
        }

        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        public void GetCommonDirectory_NoCommonString_EmptyString()
        {
            Assert.AreEqual(string.Empty, CommonDirectorySearcher.GetCommonDirectory(new[] { "C:\\", "D:\\" }));
        }

        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        public void GetCommonDirectory_SingleCharacterCommonString_CommonString()
        {
            Assert.AreEqual("C:\\", CommonDirectorySearcher.GetCommonDirectory(new[] { "C:\\a", "C:\\ab" }));
        }

        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        public void GetCommonDirectory_SeveralCharacterCommonString_CommonString()
        {
            Assert.AreEqual("C:\\abc\\", CommonDirectorySearcher.GetCommonDirectory(new[] { "C:\\abc\\1", "C:\\abc\\2", "C:\\abc\\3" }));
        }

        /// <summary>
        /// A test for GetCommonDirectory
        /// </summary>
        [TestMethod]
        public void GetCommonDirectory_DifferingCase_CommonStringCaseInsensitive()
        {
            Assert.AreEqual("C:\\abc\\", CommonDirectorySearcher.GetCommonDirectory(new[] { "C:\\Abc\\1", "C:\\abc\\2" }), true);
        }
    }
}
