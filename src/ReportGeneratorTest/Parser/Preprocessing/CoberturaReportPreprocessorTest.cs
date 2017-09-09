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
