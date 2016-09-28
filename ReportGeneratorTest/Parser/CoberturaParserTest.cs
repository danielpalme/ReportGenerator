using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for CoberturaParser and is intended
    /// to contain all CoberturaParser Unit Tests
    /// </summary>
    [TestClass]
    public class CoberturaParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");

        private static IEnumerable<Assembly> assemblies;

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileManager.CopyTestClasses();

            assemblies = new CoberturaParser(XDocument.Load(FilePath1)).Assemblies;
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            FileManager.DeleteTestClasses();
        }

        #endregion

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [TestMethod]
        public void NumberOfFilesTest()
        {
            Assert.AreEqual(6, assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [TestMethod]
        public void FilesOfClassTest()
        {
            Assert.AreEqual(1, assemblies.Single(a => a.Name == string.Empty).Classes.Single(c => c.Name == "TestClass").Files.Count(), "Wrong number of files");
            Assert.AreEqual(1, assemblies.Single(a => a.Name == string.Empty).Classes.Single(c => c.Name == "GenericClass").Files.Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [TestMethod]
        public void ClassesInAssemblyTest()
        {
            Assert.AreEqual(6, assemblies.SelectMany(a => a.Classes).Count(), "Wrong number of classes");
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [TestMethod]
        public void AssembliesTest()
        {
            Assert.AreEqual(1, assemblies.Count(), "Wrong number of assemblies");
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [TestMethod]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.AreEqual(3, assemblies.Single(a => a.Name == string.Empty).Classes.Single(c => c.Name == "AbstractClass").CoverableLines, "Wrong Coverable Lines");
        }
    }
}
