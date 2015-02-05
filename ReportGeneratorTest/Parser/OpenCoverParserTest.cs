using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for OpenCoverParser and is intended
    /// to contain all OpenCoverParser Unit Tests
    /// </summary>
    [TestClass]
    public class OpenCoverParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private static readonly string FilePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCoverWithTrackedMethods.xml");

        private static IEnumerable<Assembly> assembliesWithoutPreprocessing;

        private static IEnumerable<Assembly> assembliesWithPreprocessing;

        private static IEnumerable<Assembly> assembliesWithTrackedMethods;

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileManager.CopyTestClasses();

            assembliesWithoutPreprocessing = new OpenCoverParser(XDocument.Load(FilePath1)).Assemblies;

            var report = XDocument.Load(FilePath1);
            var classSearcherFactory = new ClassSearcherFactory();
            var globalClassSearcher = classSearcherFactory.CreateClassSearcher("C:\\test");
            new OpenCoverReportPreprocessor(report, classSearcherFactory, globalClassSearcher).Execute();
            assembliesWithPreprocessing = new OpenCoverParser(report).Assemblies;

            report = XDocument.Load(FilePath2);
            classSearcherFactory = new ClassSearcherFactory();
            globalClassSearcher = classSearcherFactory.CreateClassSearcher("C:\\test");
            new OpenCoverReportPreprocessor(report, classSearcherFactory, globalClassSearcher).Execute();
            assembliesWithTrackedMethods = new OpenCoverParser(report).Assemblies;
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            FileManager.DeleteTestClasses();
        }

        #endregion

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [TestMethod]
        public void NumberOfLineVisitsTest_WithoutPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(2, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(4, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits, "Wrong number of line visits");
            Assert.IsFalse(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.IsFalse(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 45).CoveredBranches.Value, "Wrong number of covered branches");
            Assert.AreEqual(2, fileAnalysis.Lines.Single(l => l.LineNumber == 45).TotalBranches.Value, "Wrong number of total branches");

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits (Property is excluded)");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits (Method is excluded)");
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [TestMethod]
        public void NumberOfLineVisitsTest_WithPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.AreEqual(3, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(2, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(4, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits, "Wrong number of line visits");
            Assert.IsFalse(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.IsFalse(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 45).CoveredBranches.Value, "Wrong number of covered branches");
            Assert.AreEqual(2, fileAnalysis.Lines.Single(l => l.LineNumber == 45).TotalBranches.Value, "Wrong number of total branches");

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.AreEqual(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits");
            Assert.AreEqual(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits, "Wrong number of line visits");

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits, "Wrong number of line visits (Property is excluded)");
            Assert.AreEqual(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits, "Wrong number of line visits (Method is excluded)");
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [TestMethod]
        public void NumberOfLineVisitsTest_WithTrackedMethods()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithTrackedMethods, "Test.PartialClass", "C:\\temp\\PartialClass.cs");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 9);

            Assert.AreEqual(2, line.LineVisits, "Wrong number of line visits");

            Assert.AreEqual(2, line.LineCoverageByTestMethod.Count, "Wrong number of test methods");
            Assert.AreEqual(1, line.LineCoverageByTestMethod.First().Value.LineVisits, "Wrong number of test methods");
            Assert.AreEqual(1, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisits, "Wrong number of test methods");
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [TestMethod]
        public void NumberOfFilesTest()
        {
            Assert.AreEqual(11, assembliesWithoutPreprocessing.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [TestMethod]
        public void FilesOfClassTest()
        {
            Assert.AreEqual(1, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files.Count(), "Wrong number of files");
            Assert.AreEqual(2, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count(), "Wrong number of files");
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [TestMethod]
        public void ClassesInAssemblyTest()
        {
            Assert.AreEqual(16, assembliesWithoutPreprocessing.SelectMany(a => a.Classes).Count(), "Wrong number of classes");
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [TestMethod]
        public void AssembliesTest()
        {
            Assert.AreEqual(1, assembliesWithoutPreprocessing.Count(), "Wrong number of assemblies");
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [TestMethod]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.AreEqual(4, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.AbstractClass").CoverableLines, "Wrong Coverable Lines");
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [TestMethod]
        public void GetCoverageQuotaOfClassTest()
        {
            Assert.AreEqual(50m, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClassWithAutoProperties").CoverageQuota, "Wrong coverage quota");
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [TestMethod]
        public void MethodMetricsTest()
        {
            var metrics = assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").MethodMetrics;

            Assert.AreEqual(2, metrics.Count(), "Wrong number of method metrics");
            Assert.AreEqual("System.Void Test.TestClass::SampleFunction()", metrics.First().Name, "Wrong name of method");
            Assert.AreEqual(3, metrics.First().Metrics.Count(), "Wrong number of metrics");

            Assert.AreEqual("Cyclomatic Complexity", metrics.First().Metrics.ElementAt(0).Name, "Wrong name of metric");
            Assert.AreEqual(3, metrics.First().Metrics.ElementAt(0).Value, "Wrong value of metric");
            Assert.AreEqual("Sequence Coverage", metrics.First().Metrics.ElementAt(1).Name, "Wrong name of metric");
            Assert.AreEqual(75M, metrics.First().Metrics.ElementAt(1).Value, "Wrong value of metric");
            Assert.AreEqual("Branch Coverage", metrics.First().Metrics.ElementAt(2).Name, "Wrong name of metric");
            Assert.AreEqual(60, metrics.First().Metrics.ElementAt(2).Value, "Wrong value of metric");
        }

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName)
        {
            return assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
        }
    }
}
