using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NSubstitute;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser
{
    /// <summary>
    /// This is a test class for VisualStudioParser and is intended
    /// to contain all VisualStudioParser Unit Tests for Visual Studio 2013
    /// </summary>
    [Collection("FileManager")]
    public class VisualStudioParserTest_2013
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2013.coveragexml");

        private readonly ParserResult parserResult;

        public VisualStudioParserTest_2013()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(FilePath);
            this.parserResult = new VisualStudioParser(filter, filter, filter).Parse(report);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");

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
            Assert.Equal(11, this.parserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "test.exe").Classes.Single(c => c.Name == "Test.TestClass").Files);
            Assert.Equal(2, this.parserResult.Assemblies.Single(a => a.Name == "test.exe").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count());
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(13, this.parserResult.Assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(4, this.parserResult.Assemblies.Single(a => a.Name == "test.exe").Classes.Single(c => c.Name == "Test.AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Single(this.parserResult.Assemblies);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.parserResult.Assemblies.Single(a => a.Name == "test.exe").Classes.Single(c => c.Name == "Test.TestClass").Files.Single(f => f.Path == "C:\\temp\\TestClass.cs").MethodMetrics;

            Assert.Equal(2, metrics.Count());
            Assert.Equal("SampleFunction()", metrics.First().FullName);
            Assert.Equal(2, metrics.First().Metrics.Count());

            Assert.Equal("Blocks covered", metrics.First().Metrics.ElementAt(0).Name);
            Assert.Equal(9, metrics.First().Metrics.ElementAt(0).Value);
            Assert.Equal("Blocks not covered", metrics.First().Metrics.ElementAt(1).Name);
            Assert.Equal(4, metrics.First().Metrics.ElementAt(1).Value);

            metrics = this.parserResult.Assemblies.Single(a => a.Name == "test.exe").Classes.Single(c => c.Name == "Test.AsyncClass").Files.Single(f => f.Path == "C:\\temp\\AsyncClass.cs").MethodMetrics;

            Assert.Single(metrics);
            Assert.Equal("SendAsync()", metrics.First().FullName);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.parserResult.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs").CodeElements;
            Assert.Equal(2, codeElements.Count());

            codeElements = GetFile(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs").CodeElements;
            Assert.Equal(4, codeElements.Count());

            codeElements = GetFile(this.parserResult.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs").CodeElements;
            Assert.Equal(6, codeElements.Count());

            codeElements = GetFile(this.parserResult.Assemblies, "Test.AsyncClass", "C:\\temp\\AsyncClass.cs").CodeElements;
            Assert.Single(codeElements);
            Assert.Equal("SendAsync()", codeElements.First().Name);
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test.exe").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test.exe").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));
    }
}
