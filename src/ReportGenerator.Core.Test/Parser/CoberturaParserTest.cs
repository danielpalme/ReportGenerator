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
    /// This is a test class for CoberturaParser and is intended
    /// to contain all CoberturaParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CoberturaParserTest
    {
        private static readonly string FilePathJavaReport = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
        private static readonly string FilePathCSharpReport = Path.Combine(FileManager.GetCSharpReportDirectory(), "Cobertura_coverlet.xml");

        private readonly ParserResult javaParserResult;
        private readonly ParserResult csharpParserResult;

        public CoberturaParserTest()
        {
            this.javaParserResult = ParseReport(FilePathJavaReport);

            this.csharpParserResult = ParseReport(FilePathCSharpReport);
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            Assert.True(this.javaParserResult.SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(this.javaParserResult.Assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");
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
            var fileAnalysis = GetFileAnalysis(this.javaParserResult.Assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");

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
            Assert.Equal(7, this.javaParserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.javaParserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").Files);
            Assert.Single(this.javaParserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.GenericClass").Files);
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(7, this.javaParserResult.Assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Equal(2, this.javaParserResult.Assemblies.Count);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(3, this.javaParserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.javaParserResult.Assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").Files.Single(f => f.Path == "C:\\temp\\test\\TestClass.java").MethodMetrics.ToArray();

            Assert.Equal(4, metrics.Count());

            var initMethodMetric = metrics.First();
            Assert.Equal("<init>()V", initMethodMetric.FullName);
            Assert.Equal(4, initMethodMetric.Metrics.Count());

            var complexityMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CodeQuality && m.Abbreviation == "cc");
            Assert.Equal("Cyclomatic complexity", complexityMetric.Name);
            Assert.Equal(0, complexityMetric.Value);

            var lineCoverageMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "cov");
            Assert.Equal("Line coverage", lineCoverageMetric.Name);
            Assert.Equal(100.0M, lineCoverageMetric.Value);

            var branchCoverageMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "bcov");
            Assert.Equal("Branch coverage", branchCoverageMetric.Name);
            Assert.Equal(100.0M, branchCoverageMetric.Value);

            var crapScoreMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CodeQuality && m.Abbreviation == "crp");
            Assert.Equal("Crap Score", crapScoreMetric.Name);
            Assert.Equal(0M, crapScoreMetric.Value);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Theory]
        [InlineData("Test", "Test.AbstractClass", "C:\\temp\\AbstractClass.cs", ".ctor()", 1, 1, 100, 100, 1)]
        [InlineData("Test", "Test.AbstractClass_SampleImpl1", "C:\\temp\\AbstractClass.cs", "Method1()", 3, 1, 0, 100, 2)]
        [InlineData("Test", "Test.PartialClass", "C:\\temp\\PartialClass.cs", "set_SomeProperty(System.Int32)", 4, 2, 66.66, 50, 2)]
        [InlineData("Test", "Test.Program", "C:\\temp\\Program.cs", "Main(System.String[])", 4, 1, 89.65, 100, 1.00)]
        [InlineData("Test", "Test.TestClass", "C:\\temp\\TestClass.cs", "SampleFunction()", 5, 4, 80, 50, 4)]
        public void MethodMetricsTest_2(string assemblyName, string className, string filePath, string methodName, int expectedMethodMetrics, double expectedComplexity, double expectedLineCoverage, double expectedBranchCoverage, double expectedCrapScore)
        {
            var methodMetrics = csharpParserResult
                .Assemblies.Single(a => a.Name == assemblyName)
                .Classes.Single(c => c.Name == className)
                .Files.Single(f => f.Path == filePath)
                .MethodMetrics.ToArray();

            Assert.Equal(expectedMethodMetrics, methodMetrics.Length);

            var methodMetric = methodMetrics.First(m => m.FullName == methodName);
            Assert.Equal(methodName, methodMetric.FullName);
            Assert.Equal(4, methodMetric.Metrics.Count());

            var complexityMetric = methodMetric.Metrics.Single(m => m.MetricType == MetricType.CodeQuality && m.Abbreviation == "cc");
            Assert.Equal("Cyclomatic complexity", complexityMetric.Name);
            Assert.True(complexityMetric.Value.HasValue);
            Assert.Equal((decimal)expectedComplexity, complexityMetric.Value);

            var lineCoverageMetric = methodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "cov");
            Assert.Equal("Line coverage", lineCoverageMetric.Name);
            Assert.True(lineCoverageMetric.Value.HasValue);
            Assert.Equal((decimal)expectedLineCoverage, lineCoverageMetric.Value);

            var branchCoverageMetric = methodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "bcov");
            Assert.Equal("Branch coverage", branchCoverageMetric.Name);
            Assert.True(branchCoverageMetric.Value.HasValue);
            Assert.Equal((decimal)expectedBranchCoverage, branchCoverageMetric.Value);

            var crapScoreMetric = methodMetric.Metrics.Single(m => m.MetricType == MetricType.CodeQuality && m.Abbreviation == "crp");
            Assert.Equal("Crap Score", crapScoreMetric.Name);
            Assert.True(crapScoreMetric.Value.HasValue);
            Assert.Equal((decimal)expectedCrapScore, crapScoreMetric.Value);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.javaParserResult.Assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java").CodeElements;
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

        private static ParserResult ParseReport(string filePath)
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(filePath);
            new CoberturaReportPreprocessor().Execute(report);
            return new CoberturaParser(filter, filter, filter).Parse(report.Root);
        }
    }
}
