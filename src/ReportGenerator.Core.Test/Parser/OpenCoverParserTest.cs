using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for OpenCoverParser and is intended
    /// to contain all OpenCoverParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class OpenCoverParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private static readonly string FilePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCoverWithTrackedMethods.xml");

        private static IEnumerable<Assembly> assembliesWithoutPreprocessing;

        private static IEnumerable<Assembly> assembliesWithPreprocessing;

        private static IEnumerable<Assembly> assembliesWithTrackedMethods;

        public OpenCoverParserTest()
        {
            assembliesWithoutPreprocessing = new OpenCoverParser(XDocument.Load(FilePath1)).Assemblies;

            var report = XDocument.Load(FilePath1);
            new OpenCoverReportPreprocessor(report).Execute();
            assembliesWithPreprocessing = new OpenCoverParser(report).Assemblies;

            report = XDocument.Load(FilePath2);
            new OpenCoverReportPreprocessor(report).Execute();
            assembliesWithTrackedMethods = new OpenCoverParser(report).Assemblies;
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            Assert.True(new OpenCoverParser(XDocument.Load(FilePath1)).SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithoutPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(3, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(4, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 54).TotalBranches.Value);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(3, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(4, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 54).TotalBranches.Value);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithTrackedMethods()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithTrackedMethods, "Test.PartialClass", "C:\\temp\\PartialClass.cs");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 9);

            Assert.Equal(2, line.LineVisits);

            Assert.Equal(2, line.LineCoverageByTestMethod.Count);
            Assert.Equal(1, line.LineCoverageByTestMethod.First().Value.LineVisits);
            Assert.Equal(1, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest_WithTrackedMethods()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithTrackedMethods, "Test.TestClass", "C:\\temp\\TestClass.cs");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);

            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            Assert.Equal(2, line.LineCoverageByTestMethod.Count);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineCoverageByTestMethod.First().Value.LineVisitStatus);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 15);

            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            Assert.Equal(2, line.LineCoverageByTestMethod.Count);
            Assert.Equal(LineVisitStatus.Covered, line.LineCoverageByTestMethod.First().Value.LineVisitStatus);
            Assert.Equal(LineVisitStatus.Covered, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 17);

            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineVisitStatus);

            Assert.Equal(2, line.LineCoverageByTestMethod.Count);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineCoverageByTestMethod.First().Value.LineVisitStatus);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 22);

            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);

            Assert.Equal(2, line.LineCoverageByTestMethod.Count);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineCoverageByTestMethod.First().Value.LineVisitStatus);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineCoverageByTestMethod.ElementAt(1).Value.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(15, assembliesWithoutPreprocessing.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files);
            Assert.Equal(2, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count());
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(17, assembliesWithoutPreprocessing.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Single(assembliesWithoutPreprocessing);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(4, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverageQuotaOfClassTest()
        {
            Assert.Equal(66.6m, assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClassWithAutoProperties").CoverageQuota);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").MethodMetrics;

            Assert.Equal(2, metrics.Count());
            Assert.Equal("System.Void Test.TestClass::SampleFunction()", metrics.First().Name);
            Assert.Equal(5, metrics.First().Metrics.Count());

            Assert.Equal("Cyclomatic complexity", metrics.First().Metrics.ElementAt(0).Name);
            Assert.Equal(3, metrics.First().Metrics.ElementAt(0).Value);
            Assert.Equal("NPath complexity", metrics.First().Metrics.ElementAt(1).Name);
            Assert.Equal(2, metrics.First().Metrics.ElementAt(1).Value);
            Assert.Equal("Sequence coverage", metrics.First().Metrics.ElementAt(2).Name);
            Assert.Equal(75M, metrics.First().Metrics.ElementAt(2).Value);
            Assert.Equal("Branch coverage", metrics.First().Metrics.ElementAt(3).Name);
            Assert.Equal(66.67M, metrics.First().Metrics.ElementAt(3).Value);
            Assert.Equal("Crap Score", metrics.First().Metrics.ElementAt(4).Name);
            Assert.Equal(3.14M, metrics.First().Metrics.ElementAt(4).Value);

            metrics = assembliesWithoutPreprocessing.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.AsyncClass").MethodMetrics;

            Assert.Single(metrics);
            Assert.Equal("SendAsync()", metrics.First().Name);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(assembliesWithPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs").CodeElements;
            Assert.Equal(2, codeElements.Count());

            codeElements = GetFile(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs").CodeElements;
            Assert.Equal(4, codeElements.Count());

            codeElements = GetFile(assembliesWithPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs").CodeElements;
            Assert.Equal(10, codeElements.Count());

            codeElements = GetFile(assembliesWithPreprocessing, "Test.AsyncClass", "C:\\temp\\AsyncClass.cs").CodeElements;
            Assert.Single(codeElements);
            Assert.Equal("SendAsync()", codeElements.First().Name);
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
