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

        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileManager.CopyTestClasses();

            var report = XDocument.Load(FilePath1);
            new CoberturaReportPreprocessor(report).Execute();
            assemblies = new CoberturaParser(report).Assemblies;
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            FileManager.DeleteTestClasses();
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
            var fileAnalysis = GetFileAnalysis(assemblies, "TestClass", "C:\\temp\\TestClass.java");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits, "Wrong number of line visits");
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [TestMethod]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "TestClass", "C:\\temp\\TestClass.java");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.AreEqual(LineVisitStatus.NotCoverable, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 12);
            Assert.AreEqual(LineVisitStatus.Covered, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 14);
            Assert.AreEqual(LineVisitStatus.PartiallyCovered, line.LineVisitStatus, "Wrong line visit status");

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 19);
            Assert.AreEqual(LineVisitStatus.NotCovered, line.LineVisitStatus, "Wrong line visit status");
        }

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

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == string.Empty).Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
    }
}
