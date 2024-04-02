using System.IO;
using System.Linq;
using NSubstitute;
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
        private readonly IFilter filter = Substitute.For<IFilter>();

        public CoverageReportParserTest()
        {
            this.filter.IsElementIncludedInReport(Arg.Any<string>()).Returns(true);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSingleReport_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.3.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            var parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("NCover", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("OpenCover", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("DotCover", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2010.coveragexml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("VisualStudio", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "DynamicCodeCoverage.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("DynamicCodeCoverage", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("MProf", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("Cobertura", parserResult.ParserName);
            Assert.Single(parserResult.SourceDirectories);
            Assert.Equal("C:/temp", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetCPlusPlusReportDirectory(), "Cobertura_CPPCoverage.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("Cobertura", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "JaCoCo0.8.3.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("JaCoCo", parserResult.ParserName);
            Assert.Single(parserResult.SourceDirectories);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Clover_OpenClover4.3.1.xml");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("Clover", parserResult.ParserName);
            Assert.Single(parserResult.SourceDirectories);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "lcov.info");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("LCov", parserResult.ParserName);
            Assert.Empty(parserResult.SourceDirectories);

            filePath = Path.Combine(FileManager.GetCPlusPlusReportDirectory(), "gcov", "basic", "main.cpp.gcov");
            parserResult = new CoverageReportParser(1, 1, new string[] { "C:\\somedirectory" }, this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath });
            Assert.Equal("GCov", parserResult.ParserName);
            Assert.Single(parserResult.SourceDirectories);
            Assert.Equal("C:\\somedirectory", parserResult.SourceDirectories.First());
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }));

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.2.xml");
            Assert.Throws<UnsupportedParserException>(() => new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }));
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SingleReportFileWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiNCover1.5.8.xml");
            string parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReport (2x NCover)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReport (2x OpenCover)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultidotCover.xml");
            parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReport (2x DotCover)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiVisualStudio2010.coveragexml");
            parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReport (2x VisualStudio)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiDynamicCodeCoverage.xml");
            parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath }).ParserName;
            Assert.Equal("MultiReport (2x DynamicCodeCoverage)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SeveralReportFilesWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
            Assert.Equal("MultiReport (1x NCover, 1x OpenCover)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_SeveralReportFilesWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            string parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { filePath, filePath2 }).ParserName;
            Assert.Equal("MultiReport (1x NCover, 2x OpenCover)", parserName);
        }

        /// <summary>
        /// A test for ParseFiles
        /// </summary>
        [Fact]
        public void ParseFiles_NoReports_CorrectParserIsReturned()
        {
            string parserName = new CoverageReportParser(1, 1, System.Array.Empty<string>(), this.filter, this.filter, this.filter).ParseFiles(new string[] { string.Empty }).ParserName;
            Assert.Equal(string.Empty, parserName);
        }
    }
}
