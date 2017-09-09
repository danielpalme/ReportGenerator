using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for DynamicCodeCoverageReportPreprocessor and is intended
    /// to contain all DynamicCodeCoverageReportPreprocessor Unit Tests
    /// </summary>
    [TestClass]
    public class DynamicCodeCoverageReportPreprocessorTest
    {
        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "DynamicCodeCoverage.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [TestMethod]
        public void Execute_ClassNameAddedToStartupFunctionElements()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeFunctions = report.Root
                .Elements("modules")
                .Elements("module")
                .Elements("functions")
                .Elements("function")
                .Where(c => c.Attribute("type_name").Value.StartsWith("$"))
                .ToArray();

            Assert.AreEqual(15, startupCodeFunctions.Length, "Wrong number of auto generated functions.");

            new DynamicCodeCoverageReportPreprocessor(report).Execute();

            var updatedStartupCodeFunctions = report.Root
                .Elements("modules")
                .Elements("module")
                .Elements("functions")
                .Elements("function")
                .Where(c => c.Attribute("type_name").Value.StartsWith("$"))
                .ToArray();

            Assert.AreEqual(1, updatedStartupCodeFunctions.Length, "Wrong number of auto generated functions.");

            for (int i = 1; i < 7; i++)
            {
                Assert.IsTrue(startupCodeFunctions[i].Attribute("type_name").Value.StartsWith("MouseBehavior."));
            }

            for (int i = 8; i < 15; i++)
            {
                Assert.IsTrue(startupCodeFunctions[i].Attribute("type_name").Value.StartsWith("TestMouseBehavior."));
            }
        }
    }
}
