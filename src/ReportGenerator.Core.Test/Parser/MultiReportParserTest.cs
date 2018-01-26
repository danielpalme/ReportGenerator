using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Moq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for MultiReportParser and is intended
    /// to contain all MultiReportParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class MultiReportParserTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private static IEnumerable<Assembly> assembliesWithoutPreprocessing;

        private static IEnumerable<Assembly> assembliesWithPreprocessing;

        public MultiReportParserTest()
        {
            var multiReportParser = new MultiReportParser();
            multiReportParser.AddParser(new OpenCoverParser(XDocument.Load(FilePath)));
            multiReportParser.AddParser(new OpenCoverParser(XDocument.Load(FilePath)));
            assembliesWithoutPreprocessing = multiReportParser.Assemblies;

            multiReportParser = new MultiReportParser();

            var report = XDocument.Load(FilePath);
            new OpenCoverReportPreprocessor(report).Execute();
            multiReportParser.AddParser(new OpenCoverParser(report));

            report = XDocument.Load(FilePath);
            new OpenCoverReportPreprocessor(report).Execute();
            multiReportParser.AddParser(new OpenCoverParser(report));
            assembliesWithPreprocessing = multiReportParser.Assemblies;
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            var multiReportParser = new MultiReportParser();

            Assert.False(multiReportParser.SupportsBranchCoverage);

            var mock = new Mock<IParser>();
            mock.Setup(p => p.SupportsBranchCoverage).Returns(false);
            multiReportParser.AddParser(mock.Object);

            Assert.False(multiReportParser.SupportsBranchCoverage);

            mock = new Mock<IParser>();
            mock.Setup(p => p.SupportsBranchCoverage).Returns(true);
            multiReportParser.AddParser(mock.Object);

            Assert.True(multiReportParser.SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest_WithoutPreprocessing()
        {
            var fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass", "C:\\temp\\TestClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
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

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithoutPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
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
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 10).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 11).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 12).LineVisits);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 23).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 31).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.TestClass2", "C:\\temp\\TestClass2.cs");
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

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.PartialClass", "C:\\temp\\PartialClass2.cs");
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 14).LineVisits);

            fileAnalysis = GetFileAnalysis(assembliesWithPreprocessing, "Test.ClassWithExcludes", "C:\\temp\\ClassWithExcludes.cs");
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 9).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 19).LineVisits);
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
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void OpenCoverMethodMetricsTest()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            var multiReportParser = ParserFactory.CreateParser(new string[] { filePath });
            Assert.IsType<MultiReportParser>(multiReportParser);

            var metrics = multiReportParser.Assemblies.Single(a => a.Name == "Test").Classes.Single(c => c.Name == "Test.TestClass").MethodMetrics;

            Assert.Equal(2, metrics.Count());
            Assert.Equal("System.Void Test.TestClass::SampleFunction()", metrics.First().Name);
            Assert.Equal(3, metrics.First().Metrics.Count());

            Assert.Equal("Cyclomatic complexity", metrics.First().Metrics.ElementAt(0).Name);
            Assert.Equal(111, metrics.First().Metrics.ElementAt(0).Value);
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
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            var multiReportParser = ParserFactory.CreateParser(new string[] { filePath });
            Assert.IsType<MultiReportParser>(multiReportParser);

            var fileAnalysis = GetFileAnalysis(multiReportParser.Assemblies, "Test.TestClass2", "C:\\temp\\TestClass2.cs");

            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).CoveredBranches.HasValue, "No covered branches");
            Assert.False(fileAnalysis.Lines.Single(l => l.LineNumber == 44).TotalBranches.HasValue, "No total branches");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 45).CoveredBranches.Value);
            Assert.Equal(2, fileAnalysis.Lines.Single(l => l.LineNumber == 45).TotalBranches.Value);
        }

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "Test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
    }
}
