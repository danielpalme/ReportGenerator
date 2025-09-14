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
    /// This is a test class for CtcParser and is intended
    /// to contain all CtcParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CtcParserTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCtcReportsDirectory(), "source-00001.xml");

        private readonly ParserResult parserResult;

        public CtcParserTest()
        {
            this.parserResult = ParseReport(FilePath);
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
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "PassengerScan", "C:\\temp\\PassengerScan.cpp");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 3).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 5).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "PassengerScan", "C:\\temp\\PassengerScan.cpp");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 3);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 5);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Single(this.parserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "Coasterbeispiel - XML4ReportGenerator").Classes.Single(c => c.Name == "PassengerScan").Files);
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Single(this.parserResult.Assemblies.SelectMany(a => a.Classes));
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
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(5, this.parserResult.Assemblies.Single(a => a.Name == "Coasterbeispiel - XML4ReportGenerator").Classes.Single(c => c.Name == "PassengerScan").CoverableLines);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.parserResult.Assemblies.Single(a => a.Name == "Coasterbeispiel - XML4ReportGenerator").Classes.Single(c => c.Name == "PassengerScan").Files.Single(f => f.Path == "C:\\temp\\PassengerScan.cpp").MethodMetrics.ToArray();

            Assert.Single(metrics);

            var initMethodMetric = metrics.First();
            Assert.Equal("hasAdmission", initMethodMetric.FullName);
            Assert.Equal(3, initMethodMetric.Metrics.Count());

            var mcdcCoverageMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "mcdc");
            Assert.Equal("MC/DC", mcdcCoverageMetric.Name);
            Assert.Equal(75, mcdcCoverageMetric.Value);

            var decisionCoverageMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "decision");
            Assert.Equal("Decision", decisionCoverageMetric.Name);
            Assert.Equal(100, decisionCoverageMetric.Value);

            var statementCoverageMetric = initMethodMetric.Metrics.Single(m => m.MetricType == MetricType.CoveragePercentual && m.Abbreviation == "stmt");
            Assert.Equal("Statement", statementCoverageMetric.Name);
            Assert.Equal(100, statementCoverageMetric.Value);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.parserResult.Assemblies, "PassengerScan", "C:\\temp\\PassengerScan.cpp").CodeElements;
            Assert.Single(codeElements);
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Coasterbeispiel - XML4ReportGenerator").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Coasterbeispiel - XML4ReportGenerator").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));

        private static ParserResult ParseReport(string filePath)
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(filePath);
            new CoberturaReportPreprocessor().Execute(report);
            return new CtcParser(filter, filter, filter).Parse(report.Root);
        }
    }
}
