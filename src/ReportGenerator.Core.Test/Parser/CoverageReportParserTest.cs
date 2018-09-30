using System.IO;
using Moq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
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
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("NCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("OpenCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("DotCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2010.coveragexml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("VisualStudioParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "DynamicCodeCoverage.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("DynamicCodeCoverageParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MProfParser", parserName);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("CoberturaParser", parserName);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "JaCoCo0.8.3.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("JaCoCoParser", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiNCover1.5.8.xml");
            string parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x NCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x OpenCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultidotCover.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x DotCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiVisualStudio2010.coveragexml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReportParser (2x VisualStudioParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiDynamicCodeCoverage.xml");
            parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath }).ParserName;
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
            string parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
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
            string parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
            Assert.Equal("MultiReportParser (1x NCoverParser, 2x OpenCoverParser)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_NoReports_CorrectParserIsReturned()
        {
            string parserName = new CoverageReportParser(new string[0], this.filterMock.Object, this.filterMock.Object, this.filterMock.Object).ParseFiles(new string[] { string.Empty }).ParserName;
            Assert.Equal(string.Empty, parserName);
        }
    }
}
