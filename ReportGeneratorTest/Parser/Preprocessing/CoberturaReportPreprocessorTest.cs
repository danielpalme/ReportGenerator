using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for CoberturaReportPreprocessor and is intended
    /// to contain all CoberturaReportPreprocessor Unit Tests
    /// </summary>
    [TestClass]
    public class CoberturaReportPreprocessorTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileManager.CopyTestClasses();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            FileManager.DeleteTestClasses();
        }

        #endregion

        /// <summary>
        /// A test for Execute
        /// </summary>
        [TestMethod]
        public void Execute_FullFilePathApplied()
        {
            XDocument report = XDocument.Load(FilePath);

            new CoberturaReportPreprocessor(report).Execute();

            var filesPaths = report.Root
                .Elements("packages")
                .Elements("package")
                .Elements("classes")
                .Elements("class")
                .Select(c => c.Attribute("filename").Value)
                .ToArray();

            Assert.IsTrue(filesPaths.Length > 0);
            Assert.IsTrue(filesPaths.All(f => f.StartsWith("C:\\temp\\")));
        }
    }
}
