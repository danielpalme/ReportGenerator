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
    /// This is a test class for MultiReportParser and is intended
    /// to contain all ParserResultTest Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class ParserResultTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private readonly ParserResult parserResultWithoutPreprocessing;

        private readonly ParserResult parserResultWithPreprocessing;

        public ParserResultTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            this.parserResultWithoutPreprocessing = new OpenCoverParser(filter, filter, filter).Parse(XDocument.Load(FilePath));
            this.parserResultWithoutPreprocessing.Merge(new OpenCoverParser(filter, filter, filter).Parse(XDocument.Load(FilePath)));

            var report = XDocument.Load(FilePath);
            new OpenCoverReportPreprocessor().Execute(report);
            this.parserResultWithPreprocessing = new OpenCoverParser(filter, filter, filter).Parse(report);

            report = XDocument.Load(FilePath);
            new OpenCoverReportPreprocessor().Execute(report);
            this.parserResultWithPreprocessing.Merge(new OpenCoverParser(filter, filter, filter).Parse(report));
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            var parserResult = new ParserResult();
            Assert.False(parserResult.SupportsBranchCoverage);

            parserResult = new ParserResult(new List<Assembly>(), false, "Test");
            Assert.False(parserResult.SupportsBranchCoverage);

            parserResult.Merge(new ParserResult(new List<Assembly>(), true, "Test"));
            Assert.True(parserResult.SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for SourceDirectories
        /// </summary>
        [Fact]
        public void SourceDirectories()
        {
            var parserResult1 = new ParserResult();
            Assert.Empty(parserResult1.SourceDirectories);

            parserResult1.AddSourceDirectory("C:\\temp1");
            parserResult1.AddSourceDirectory("C:\\temp2");
            Assert.Equal(2, parserResult1.SourceDirectories.Count);

            var parserResult2 = new ParserResult();
            parserResult2.AddSourceDirectory("C:\\temp2");
            parserResult2.AddSourceDirectory("C:\\temp3");
            Assert.Equal(2, parserResult1.SourceDirectories.Count);

            parserResult1.Merge(parserResult2);
            Assert.Equal(3, parserResult1.SourceDirectories.Count);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithoutPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResultWithoutPreprocessing.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithoutPreprocessing.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(6, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(4, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(8, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 54).TotalBranches.Value);

            fileAnalysis = GetFileAnalysis(this.parserResultWithoutPreprocessing.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithoutPreprocessing.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithoutPreprocessing.Assemblies, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(this.parserResultWithPreprocessing.Assemblies, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithPreprocessing.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
            Assert.Equal(6, fileAnalysis.Lines.Single(l => l.LineNumber == 13).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(4, fileAnalysis.Lines.Single(l => l.LineNumber == 25).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 37).LineVisits);
            Assert.Equal(8, fileAnalysis.Lines.Single(l => l.LineNumber == 54).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 81).LineVisits);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue);
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 54).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 54).TotalBranches.Value);

            fileAnalysis = GetFileAnalysis(this.parserResultWithPreprocessing.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithPreprocessing.Assemblies, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(this.parserResultWithPreprocessing.Assemblies, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(15, this.parserResultWithoutPreprocessing.Assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(this.parserResultWithoutPreprocessing.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files);
            Assert.Equal(2, this.parserResultWithoutPreprocessing.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.PartialClass").Files.Count());
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(17, this.parserResultWithoutPreprocessing.Assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Single(this.parserResultWithoutPreprocessing.Assemblies);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = this.parserResultWithoutPreprocessing.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").Files.Single(f => f.Path == "C:\\temp\\TestClass.cs").MethodMetrics;

            Assert.Equal(2, metrics.Count());
            Assert.Equal("System.Void Test.TestClass::SampleFunction()", metrics.First().FullName);
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

            metrics = this.parserResultWithoutPreprocessing.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.AsyncClass").Files.Single(f => f.Path == "C:\\temp\\AsyncClass.cs").MethodMetrics;

            Assert.Single(metrics);
            Assert.Equal("SendAsync()", metrics.First().FullName);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void OpenCoverMethodMetricsTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            var parserResult = new CoverageReportParser(1, 1, System.Array.Empty<string>(), filter, filter, filter).ParseFiles(new string[] { filePath });

            var metrics = parserResult.Assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == "Test.TestClass").Files
                .Single(f => f.Path == "C:\\temp\\TestClass.cs")
                .MethodMetrics;

            Assert.Equal(2, metrics.Count());
            Assert.Equal("System.Void Test.TestClass::SampleFunction()", metrics.First().FullName);
            Assert.Equal(3, metrics.First().Metrics.Count());

            Assert.Equal("Cyclomatic complexity", metrics.First().Metrics.ElementAt(0).Name);
            Assert.Equal(3, metrics.First().Metrics.ElementAt(0).Value);
            Assert.Equal("Sequence coverage", metrics.First().Metrics.ElementAt(1).Name);
            Assert.Equal(222, metrics.First().Metrics.ElementAt(1).Value);
            Assert.Equal("Branch coverage", metrics.First().Metrics.ElementAt(2).Name);
            Assert.Equal(333, metrics.First().Metrics.ElementAt(2).Value);
        }

        /// <summary>
        /// A test for branches
        /// </summary>
        [Fact]
        public void OpenCoverBranchesTest()
        {
            var filter = Substitute.For<IFilter>();
            filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);

            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            var parserResult = new CoverageReportParser(1, 1, System.Array.Empty<string>(), filter, filter, filter).ParseFiles(new string[] { filePath });

            var fileAnalysis = GetFileAnalysis(parserResult.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");

            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 45).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 45).TotalBranches.Value);
        }

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));
    }
}
