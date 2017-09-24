using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for DotCoverReportPreprocessor and is intended
    /// to contain all DotCoverReportPreprocessor Unit Tests
    /// </summary>
    [TestClass]
    public class DotCoverReportPreprocessorTest
    {
        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "dotCover.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [TestMethod]
        public void Execute_MoveStartupCodeElementsToParentType()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeClasses = report.Root
                .Elements("Assembly")
                .Elements("Namespace")
                .Where(c => c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .Where(t => t.Attribute("Name").Value.StartsWith("$Module", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .ToArray();

            Assert.AreEqual(14, startupCodeClasses.Length, "Wrong number of auto generated classes.");

            var classSearcherFactory = new ClassSearcherFactory();
            new DotCoverReportPreprocessor(report).Execute();

            var updatedStartupCodeClasses = report.Root
                .Elements("Assembly")
                .Elements("Namespace")
                .Where(c => c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .Where(t => t.Attribute("Name").Value.StartsWith("$Module", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .ToArray();

            Assert.AreEqual(0, updatedStartupCodeClasses.Length, "Wrong number of auto generated classes.");

            for (int i = 3; i < 7; i++)
            {
                Assert.IsTrue(startupCodeClasses[i].Parent.Attribute("Name").Value.StartsWith("MouseBehavior"));
            }

            for (int i = 8; i < 13; i++)
            {
                Assert.IsTrue(startupCodeClasses[i].Parent.Attribute("Name").Value.StartsWith("TestMouseBehavior"));
            }
        }
    }
}
