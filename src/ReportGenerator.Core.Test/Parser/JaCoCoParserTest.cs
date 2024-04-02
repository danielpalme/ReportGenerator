using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NSubstitute;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser
{
    /// <summary>
    /// This is a test class for JaCoCoParser and is intended
    /// to contain all JaCoCoParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class JaCoCoParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetJavaReportDirectory(), "JaCoCo0.8.3.xml");

        private readonly ParserResult parserResult;

        public JaCoCoParserTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(FilePath1);
            new JaCoCoReportPreprocessor(new[] { "C:\\temp" }).Execute(report);
            this.parserResult = new JaCoCoParser(filter, filter, filter).Parse(report);
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            Assert.True(this.parserResult.SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "test/TestClass", "C:\\temp\\test\\TestClass.java");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 17).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 20).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "test/TestClass", "C:\\temp\\test\\TestClass.java");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 12);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 15);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 20);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(7, this.parserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test/TestClass").Files);
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test/GenericClass").Files);
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(7, this.parserResult.Assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Equal(2, this.parserResult.Assemblies.Count());
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(3, this.parserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test/AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.parserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test/TestClass").Files.Single(f => f.Path == "C:\\temp\\test\\TestClass.java").MethodMetrics;

            Assert.Equal(4, metrics.Count());
            Assert.Equal("<init>()V", metrics.ElementAt(2).FullName);
            Assert.Equal(2, metrics.ElementAt(2).Metrics.Count());

            Assert.Equal("Line coverage", metrics.ElementAt(2).Metrics.ElementAt(0).Name);
            Assert.Equal(100.0M, metrics.ElementAt(2).Metrics.ElementAt(0).Value);
            Assert.Equal("Branch coverage", metrics.ElementAt(2).Metrics.ElementAt(1).Name);
            Assert.Null(metrics.ElementAt(2).Metrics.ElementAt(1).Value);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.parserResult.Assemblies, "test/TestClass", "C:\\temp\\test\\TestClass.java").CodeElements;
            Assert.Equal(4, codeElements.Count());
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));
    }
}
