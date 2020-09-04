using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for CoberturaReportPreprocessor and is intended
    /// to contain all CoberturaReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CoberturaReportPreprocessorTest
    {
        private static readonly string SingleSourceFilePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");

        private static readonly string MultiSourceFilePath = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1-MultiSource.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_FullFilePathApplied()
        {
            XDocument report = XDocument.Load(SingleSourceFilePath);

            new CoberturaReportPreprocessor().Execute(report);

            var filesPaths = report.Root
                .Elements("packages")
                .Elements("package")
                .Elements("classes")
                .Elements("class")
                .Select(c => c.Attribute("filename").Value)
                .ToArray();

            Assert.True(filesPaths.Length > 0);
            Assert.True(filesPaths.All(f => f.StartsWith("C:\\temp\\")));
        }

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_FullFilePathApplied_MultiSource()
        {
            XDocument report = XDocument.Load(MultiSourceFilePath);

            new CoberturaReportPreprocessor().Execute(report);

            var filesPaths = report.Root
                .Elements("packages")
                .Elements("package")
                .Elements("classes")
                .Elements("class")
                .Select(c => c.Attribute("filename").Value)
                .ToArray();

            Assert.True(filesPaths.Length > 0);
            Assert.True(filesPaths.All(f => f.StartsWith("C:\\temp\\")));
        }
    }
}
