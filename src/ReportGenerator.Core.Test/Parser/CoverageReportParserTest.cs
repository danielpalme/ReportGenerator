using System.IO;
using System.Linq;
using Moq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser
{
    /// <summary>
    /// This is a test class for CoverageReportParser and is intended
    /// to contain all CoverageReportParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CoverageReportParserTest
    {
        private Mock<IFilter> filterMock = new Mock<IFilter>();

        public CoverageReportParserTest()
        {
            this.filterMock.Setup(f => f.IsElementIncludedInReport(It.IsAny<string>())).Returns(true);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSingleReport_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.3.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            var parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("NCoverParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("OpenCoverParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("DotCoverParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2010.coveragexml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("VisualStudioParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "DynamicCodeCoverage.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("DynamicCodeCoverageParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("MProfParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("CoberturaParser", parserResult.ParserName);
            Assert.Equal(1, parserResult.SourceDirectories.Count);
            Assert.Equal("C:/temp", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "JaCoCo0.8.3.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("JaCoCoParser", parserResult.ParserName);
            Assert.Equal(1, parserResult.SourceDirectories.Count);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Clover_OpenClover4.3.1.xml");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("CloverParser", parserResult.ParserName);
            Assert.Equal(1, parserResult.SourceDirectories.Count);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "lcov.info");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("LCovParser", parserResult.ParserName);
            Assert.Equal(0, parserResult.SourceDirectories.Count);

            filePath = Path.Combine(FileManager.GetCPlusPlusReportDirectory(), "gcov", "basic", "main.cpp.gcov");
            parserResult = new CoverageReportParser(1, new string[] { "C:\\somedirectory" }, this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath });
            Assert.Equal("GCovParser", parserResult.ParserName);
            Assert.Equal(1, parserResult.SourceDirectories.Count);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiNCover1.5.8.xml");
            string parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x NCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x OpenCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultidotCover.xml");
            parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x DotCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiVisualStudio2010.coveragexml");
            parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x VisualStudioParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiDynamicCodeCoverage.xml");
            parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x DynamicCodeCoverageParser)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SeveralReportFilesWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
            Assert.Equal("MultiReportParser (1x NCoverParser, 1x OpenCoverParser)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SeveralReportFilesWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            string parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
            Assert.Equal("MultiReportParser (1x NCoverParser, 2x OpenCoverParser)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_NoReports_CorrectParserIsReturned()
        {
            string parserName = new CoverageReportParser(1, new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { string.Empty }).ParserName;
            Assert.Equal(string.Empty, parserName);
        }
    }
}
