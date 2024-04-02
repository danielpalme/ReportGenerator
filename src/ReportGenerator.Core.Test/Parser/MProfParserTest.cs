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
    /// This is a test class for MProfParser and is intended
    /// to contain all MProfParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class MProfParserTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");

        private readonly ParserResult parserResult;

        public MProfParserTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            var report = XDocument.Load(FilePath);
            this.parserResult = new MProfParser(filter, filter, filter).Parse(report);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 18).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);
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

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 9);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 23);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(8, this.parserResult.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.parserResult.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files);
            Assert.Equal(2, this.parserResult.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count());
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(12, this.parserResult.Assemblies.SelectMany(a => a.Classes).Count());
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
            Assert.Empty(this.parserResult.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files.Single(f => f.Path == "C:\\temp\\TestClass.cs").MethodMetrics);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(this.parserResult.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs").CodeElements;
            Assert.Single(codeElements);

            codeElements = GetFile(this.parserResult.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs").CodeElements;
            Assert.Equal(2, codeElements.Count());

            codeElements = GetFile(this.parserResult.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs").CodeElements;
            Assert.Equal(3, codeElements.Count());
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));
    }
}
