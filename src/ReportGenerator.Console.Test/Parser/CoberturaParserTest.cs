using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Parser.Preprocessing;

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

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            var report = XDocument.Load(FilePath1);
            new CoberturaReportPreprocessor(report).Execute();
            assemblies = new CoberturaParser(report).Assemblies;
        }

        #endregion

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [TestMethod]
        public void SupportsBranchCoverage()
        {
            Assert.IsTrue(new CoberturaParser(XDocument.Load(FilePath1)).SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [TestMethod]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 17).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 20).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits, "Wrong number of line visits");
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [TestMethod]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.AreEqual(LineVisitStatus.NotCoverable, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 12);
            Assert.AreEqual(LineVisitStatus.Covered, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 15);
            Assert.AreEqual(LineVisitStatus.PartiallyCovered, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 20);
            Assert.AreEqual(LineVisitStatus.NotCovered, line.LineVisitStatus, "Wrong line visit status");
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [TestMethod]
        public void NumberOfFilesTest()
        {
            Assert.AreEqual(7, assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [TestMethod]
        public void FilesOfClassTest()
        {
            Assert.AreEqual(1, assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").Files.Count(), "Wrong number of files");
            Assert.AreEqual(1, assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.GenericClass").Files.Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [TestMethod]
        public void ClassesInAssemblyTest()
        {
            Assert.AreEqual(7, assemblies.SelectMany(a => a.Classes).Count(), "Wrong number of classes");
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [TestMethod]
        public void AssembliesTest()
        {
            Assert.AreEqual(2, assemblies.Count(), "Wrong number of assemblies");
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [TestMethod]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.AreEqual(3, assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.AbstractClass").CoverableLines, "Wrong Coverable Lines");
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [TestMethod]
        public void MethodMetricsTest()
        {
            var metrics = assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").MethodMetrics;

            Assert.AreEqual(4, metrics.Count(), "Wrong number of method metrics");
            Assert.AreEqual("<init>()V", metrics.First().Name, "Wrong name of method");
            Assert.AreEqual(3, metrics.First().Metrics.Count(), "Wrong number of metrics");

            Assert.AreEqual("Cyclomatic complexity", metrics.First().Metrics.ElementAt(0).Name, "Wrong name of metric");
            Assert.AreEqual(0, metrics.First().Metrics.ElementAt(0).Value, "Wrong value of metric");
            Assert.AreEqual("Line coverage", metrics.First().Metrics.ElementAt(1).Name, "Wrong name of metric");
            Assert.AreEqual(1.0M, metrics.First().Metrics.ElementAt(1).Value, "Wrong value of metric");
            Assert.AreEqual("Branch coverage", metrics.First().Metrics.ElementAt(2).Name, "Wrong name of metric");
            Assert.AreEqual(1.0M, metrics.First().Metrics.ElementAt(2).Value, "Wrong value of metric");
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [TestMethod]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java").CodeElements;
            Assert.AreEqual(4, codeElements.Count(), "Wrong number of code elements");
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
    }
}
