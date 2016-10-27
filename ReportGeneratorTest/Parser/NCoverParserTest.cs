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
    /// This is a test class for NCoverParser and is intended
    /// to contain all NCoverParser Unit Tests
    /// </summary>
    [TestClass]
    public class NCoverParserTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");

        private static IEnumerable<Assembly> assemblies;

        #region Additional test attributes
        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            var report = XDocument.Load(FilePath);
            assemblies = new NCoverParser(report).Assemblies;
        }

        #endregion

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [TestMethod]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 18).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(2, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(4, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [TestMethod]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.AreEqual(LineVisitStatus.NotCoverable, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 10);
            Assert.AreEqual(LineVisitStatus.Covered, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 18);
            Assert.AreEqual(LineVisitStatus.NotCovered, line.LineVisitStatus, "Wrong line visit status");
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [TestMethod]
        public void NumberOfFilesTest()
        {
            Assert.AreEqual(5, assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [TestMethod]
        public void FilesOfClassTest()
        {
            Assert.AreEqual(1, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files.Count(), "Wrong number of files");
            Assert.AreEqual(2, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [TestMethod]
        public void ClassesInAssemblyTest()
        {
            Assert.AreEqual(4, assemblies.SelectMany(a => a.Classes).Count(), "Wrong number of classes");
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
        /// A test for MethodMetrics
        /// </summary>
        [TestMethod]
        public void MethodMetricsTest()
        {
            Assert.AreEqual(0, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").MethodMetrics.Count(), "Wrong number of metrics");
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [TestMethod]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs").CodeElements;
            Assert.AreEqual(2, codeElements.Count(), "Wrong number of code elements");

            codeElements = GetFile(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs").CodeElements;
            Assert.AreEqual(4, codeElements.Count(), "Wrong number of code elements");

            codeElements = GetFile(assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs").CodeElements;
            Assert.AreEqual(5, codeElements.Count(), "Wrong number of code elements");
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
    }
}
