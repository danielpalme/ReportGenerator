using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for JaCoCoReportPreprocessor and is intended
    /// to contain all JaCoCoReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class JaCoCoReportPreprocessorTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetJavaReportDirectory(), "JaCoCo0.8.3.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_FullFilePathApplied()
        {
            XDocument report = XDocument.Load(FilePath);

            new JaCoCoReportPreprocessor(new[] { "C:\\temp\\" }).Execute(report);

            var sourcefilenameAttributesOfClasses = report.Root.Element("package").Elements("class")
                .Select(e => e.Attribute("sourcefilename").Value)
                .ToArray();

            Assert.True(sourcefilenameAttributesOfClasses.Length > 0);
            Assert.True(sourcefilenameAttributesOfClasses.All(f => f.StartsWith("C:\\temp\\")));

            var nameAttributesOfSourceFiles = report.Root.Element("package").Elements("sourcefile")
                .Select(e => e.Attribute("name").Value)
                .ToArray();

            Assert.True(nameAttributesOfSourceFiles.Length > 0);
            Assert.True(nameAttributesOfSourceFiles.All(f => f.StartsWith("C:\\temp\\")));

        }
    }
}
