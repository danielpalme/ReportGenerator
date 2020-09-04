using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for DynamicCodeCoverageReportPreprocessor and is intended
    /// to contain all DynamicCodeCoverageReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class DynamicCodeCoverageReportPreprocessorTest
    {
        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "DynamicCodeCoverage.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_ClassNameAddedToStartupFunctionElements()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeFunctions = report.Root
                .Elements("modules")
                .Elements("module")
                .Elements("functions")
                .Elements("function")
                .Where(c => c.Attribute("type_name").Value.StartsWith("$"))
                .ToArray();

            Assert.Equal(15, startupCodeFunctions.Length);

            new DynamicCodeCoverageReportPreprocessor().Execute(report);

            var updatedStartupCodeFunctions = report.Root
                .Elements("modules")
                .Elements("module")
                .Elements("functions")
                .Elements("function")
                .Where(c => c.Attribute("type_name").Value.StartsWith("$"))
                .ToArray();

            Assert.Single(updatedStartupCodeFunctions);

            for (int i = 1; i < 7; i++)
            {
                Assert.StartsWith("MouseBehavior.", startupCodeFunctions[i].Attribute("type_name").Value);
            }

            for (int i = 8; i < 15; i++)
            {
                Assert.StartsWith("TestMouseBehavior.", startupCodeFunctions[i].Attribute("type_name").Value);
            }
        }
    }
}
