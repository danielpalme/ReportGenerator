using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for DotCoverParser and is intended
    /// to contain all DotCoverParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class DotCoverParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");

        private static IEnumerable<Assembly> assemblies;

        public DotCoverParserTest()
        {
            assemblies = new DotCoverParser(XDocument.Load(FilePath1)).Assemblies;
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);

            fileAnalysis = GetFileAnalysis(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 15);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 22);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(15, assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files);
            Assert.Equal(2, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count());
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(18, assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Single(assemblies);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(4, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverageQuotaOfClassTest()
        {
            Assert.Equal(66.6m, assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClassWithAutoProperties").CoverageQuota);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            Assert.Empty(assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").MethodMetrics);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs").CodeElements;
            Assert.Equal(2, codeElements.Count());

            codeElements = GetFile(assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs").CodeElements;
            Assert.Equal(4, codeElements.Count());

            codeElements = GetFile(assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs").CodeElements;
            Assert.Equal(10, codeElements.Count());

            codeElements = GetFile(assemblies, "Test.AsyncClass", "C:\\temp\\AsyncClass.cs").CodeElements;
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
