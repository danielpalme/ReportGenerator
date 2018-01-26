using System;
using System.IO;
using Palmmedia.ReportGenerator.Core.Parser;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for ParserFactory and is intended
    /// to contain all ParserFactory Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class ParserFactoryTest
    {
        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SingleReportFileWithSingleReport_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.3.xml");
            Assert.Throws<InvalidOperationException>(() => ParserFactory.CreateParser(new string[] { filePath }).ToString());

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            Assert.Throws<InvalidOperationException>(() => ParserFactory.CreateParser(new string[] { filePath }).ToString());
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SingleReportFileWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("NCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("OpenCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("DotCoverParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2010.coveragexml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("VisualStudioParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "DynamicCodeCoverage.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("DynamicCodeCoverageParser", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MProfParser", parserName);

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("CoberturaParser", parserName);
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SingleReportFileWithSeveralReports_PartCoverNotSupported()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            Assert.Throws<InvalidOperationException>(() => ParserFactory.CreateParser(new string[] { filePath }).ToString());

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.2.xml");
            Assert.Throws<InvalidOperationException>(() => ParserFactory.CreateParser(new string[] { filePath }).ToString());
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SingleReportFileWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiNCover1.5.8.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MultiReportParser (2x NCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MultiReportParser (2x OpenCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultidotCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MultiReportParser (2x DotCoverParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiVisualStudio2010.coveragexml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MultiReportParser (2x VisualStudioParser)", parserName);

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiDynamicCodeCoverage.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }).ToString();
            Assert.Equal("MultiReportParser (2x DynamicCodeCoverageParser)", parserName);
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SeveralReportFilesWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath, filePath2 }).ToString();
            Assert.Equal("MultiReportParser (1x NCoverParser, 1x OpenCoverParser)", parserName);
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_SeveralReportFilesWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath, filePath2 }).ToString();
            Assert.Equal("MultiReportParser (1x NCoverParser, 2x OpenCoverParser)", parserName);
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [Fact]
        public void CreateParser_NoReports_CorrectParserIsReturned()
        {
            string parserName = ParserFactory.CreateParser(new string[] { string.Empty }).ToString();
            Assert.Equal(string.Empty, parserName);
        }
    }
}
