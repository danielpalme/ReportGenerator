using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// This is a test class for GCovParser and is intended
    /// to contain all GCovParserUnit Tests
    /// </summary>
    [Collection("FileManager")]
    public class GCovParserTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCPlusPlusReportDirectory(), "gcov", "branch_unconditional", "main.cpp.gcov");

        private readonly ParserResult parserResult;

        public GCovParserTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var lines = File.ReadAllLines(FilePath);
            new GCovReportPreprocessor(new[] { "C:\\temp" }).Execute(lines);
            this.parserResult = new GCovParser(filter, filter, filter).Parse(lines);
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
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "C:\\temp\\main.cpp", "C:\\temp\\main.cpp");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 3).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 8).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "C:\\temp\\main.cpp", "C:\\temp\\main.cpp");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 3);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 8);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
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
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "Default").Classes.Single(c => c.Name == "C:\\temp\\main.cpp").Files);
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
            Assert.Equal(11, this.parserResult.Assemblies.Single(a => a.Name == "Default").Classes.Single(c => c.Name == "C:\\temp\\main.cpp").CoverableLines);
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverageQuotaOfClassTest()
        {
            Assert.Equal(81.8m, this.parserResult.Assemblies.Single(a => a.Name == "Default").Classes.Single(c => c.Name == "C:\\temp\\main.cpp").CoverageQuota);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            Assert.Empty(this.parserResult.Assemblies.Single(a => a.Name == "Default").Classes.Single(c => c.Name == "C:\\temp\\main.cpp").Files.Single(f => f.Path == "C:\\temp\\main.cpp").MethodMetrics);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.parserResult.Assemblies, "C:\\temp\\main.cpp", "C:\\temp\\main.cpp").CodeElements;
            Assert.Equal(2, codeElements.Count());
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Default").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Default").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));
    }
}
