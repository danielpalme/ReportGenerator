using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for ParserFactory and is intended
    /// to contain all ParserFactory Unit Tests
    /// </summary>
    [TestClass]
    public class ParserFactoryTest
    {
        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [TestMethod]
        public void CreateParser_SingleReportFileWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.3.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("PartCover23Parser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("PartCover22Parser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "NCover1.5.8.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("NCoverParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("OpenCoverParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "dotCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("DotCoverParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "VisualStudio2010.coveragexml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("VisualStudioParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "DynamicCodeCoverage.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("DynamicCodeCoverageParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "mprof-report.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MProfParser", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("CoberturaParser", parserName, "Wrong parser");
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [TestMethod]
        public void CreateParser_SingleReportFileWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x PartCover23Parser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.2.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x PartCover22Parser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiNCover1.5.8.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x NCoverParser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiOpenCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x OpenCoverParser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultidotCover.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x DotCoverParser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiVisualStudio2010.coveragexml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x VisualStudioParser)", parserName, "Wrong parser");

            filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiDynamicCodeCoverage.xml");
            parserName = ParserFactory.CreateParser(new string[] { filePath }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (2x DynamicCodeCoverageParser)", parserName, "Wrong parser");
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [TestMethod]
        public void CreateParser_SeveralReportFilesWithSingleReport_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.3.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath, filePath2 }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (1x PartCover22Parser, 1x PartCover23Parser)", parserName, "Wrong parser");
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [TestMethod]
        public void CreateParser_SeveralReportFilesWithSeveralReports_CorrectParserIsReturned()
        {
            string filePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");
            string filePath2 = Path.Combine(FileManager.GetCSharpReportDirectory(), "MultiPartcover2.3.xml");
            string parserName = ParserFactory.CreateParser(new string[] { filePath, filePath2 }, new string[] { }).ToString();
            Assert.AreEqual("MultiReportParser (1x PartCover22Parser, 2x PartCover23Parser)", parserName, "Wrong parser");
        }

        /// <summary>
        /// A test for CreateParser
        /// </summary>
        [TestMethod]
        public void CreateParser_NoReports_CorrectParserIsReturned()
        {
            string parserName = ParserFactory.CreateParser(new string[] { string.Empty }, new string[] { }).ToString();
            Assert.AreEqual(string.Empty, parserName, "Wrong parser");
        }
    }
}
