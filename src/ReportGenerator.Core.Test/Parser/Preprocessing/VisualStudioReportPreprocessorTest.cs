using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for VisualStudioReportPreprocessor and is intended
    /// to contain all VisualStudioReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class VisualStudioReportPreprocessorTest
    {
        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "VisualStudio2010.coveragexml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_ClassNameAddedToStartupCodeElements()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeClasses = report.Root
                .Elements("Module")
                .Elements("NamespaceTable")
                .Where(c => c.Element("NamespaceName").Value.StartsWith("<StartupCode$"))
                .Elements("Class")
                .ToArray();

            Assert.Equal(15, startupCodeClasses.Length);

            new VisualStudioReportPreprocessor().Execute(report);

            var updatedStartupCodeClasses = report.Root
                .Elements("Module")
                .Elements("NamespaceTable")
                .Where(c => c.Element("NamespaceName").Value.StartsWith("<StartupCode$"))
                .Elements("Class")
                .ToArray();

            Assert.Empty(updatedStartupCodeClasses);

            Assert.Equal("$Module1", startupCodeClasses[0].Element("ClassName").Value);

            foreach (int index in new[] { 5, 6, 8, 9, 10, 11, 13 })
            {
                Assert.Equal("MouseBehavior", startupCodeClasses[index].Element("ClassName").Value);
                Assert.Equal("ViewModels", startupCodeClasses[index].Parent.Element("NamespaceName").Value);
            }

            foreach (int index in new[] { 1, 2, 3, 4, 7, 12, 14 })
            {
                Assert.Equal("TestMouseBehavior", startupCodeClasses[index].Element("ClassName").Value);
                Assert.Equal("ViewModels", startupCodeClasses[index].Parent.Element("NamespaceName").Value);
            }
        }
    }
}
