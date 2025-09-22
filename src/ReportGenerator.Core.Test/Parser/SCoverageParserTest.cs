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
    /// This is a test class for CoberturaParser and is intended
    /// to contain all CoberturaParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class SCoverageParserTest
    {
        private static readonly string FilePathScalaReport = Path.Combine(FileManager.GetScalaReportDirectory(), "scoverage.xml");

        private readonly ParserResult scalaParserResult;

        public SCoverageParserTest()
        {
            this.scalaParserResult = ParseReport(FilePathScalaReport);
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            Assert.True(this.scalaParserResult.SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(this.scalaParserResult.Assemblies, "org.scoverage.samples.RandomQuoteGenerator", "C:\\temp\\src\\main\\scala\\org\\scoverage\\samples\\QuoteGenerator.scala");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 22).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 30).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(this.scalaParserResult.Assemblies, "org.scoverage.samples.RandomQuoteGenerator", "C:\\temp\\src\\main\\scala\\org\\scoverage\\samples\\QuoteGenerator.scala");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 22);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 30);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 12);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(16, this.scalaParserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.scalaParserResult.Assemblies.Single(a => a.Name == "org.scoverage.samples").Classes.Single(c => c.Name == "org.scoverage.samples.RandomQuoteGenerator").Files);
            Assert.Single(this.scalaParserResult.Assemblies.Single(a => a.Name == "org.scoverage.samples").Classes.Single(c => c.Name == "org.scoverage.samples.InstrumentLoader").Files);
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(17, this.scalaParserResult.Assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Equal(3, this.scalaParserResult.Assemblies.Count);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(12, this.scalaParserResult.Assemblies.Single(a => a.Name == "org.scoverage.samples").Classes.Single(c => c.Name == "org.scoverage.samples.RandomQuoteGenerator").CoverableLines);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.scalaParserResult.Assemblies.Single(a => a.Name == "org.scoverage.samples").Classes.Single(c => c.Name == "org.scoverage.samples.RandomQuoteGenerator").Files.Single(f => f.Path == "C:\\temp\\src\\main\\scala\\org\\scoverage\\samples\\QuoteGenerator.scala").MethodMetrics.ToArray();

            Assert.Equal(2, metrics.Count());

            var generateMethodMetric = metrics.First();
            Assert.Equal("org.scoverage.samples/RandomQuoteGenerator/generate", generateMethodMetric.FullName);
            Assert.Equal(2, generateMethodMetric.Metrics.Count());

            var statementCoverageMetric = generateMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "stmt");
            Assert.Equal("Statement", statementCoverageMetric.Name);
            Assert.Equal(88.89M, statementCoverageMetric.Value);

            var branchCoverageMetric = generateMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "bcov");
            Assert.Equal("Branch coverage", branchCoverageMetric.Name);
            Assert.Equal(50M, branchCoverageMetric.Value);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.scalaParserResult.Assemblies, "org.scoverage.samples.RandomQuoteGenerator", "C:\\temp\\src\\main\\scala\\org\\scoverage\\samples\\QuoteGenerator.scala").CodeElements;
            Assert.Equal(2, codeElements.Count());
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "org.scoverage.samples").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "org.scoverage.samples").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));

        private static ParserResult ParseReport(string filePath)
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(filePath);
            return new SCoverageParser(filter, filter, filter).Parse(report.Root);
        }
    }
}
