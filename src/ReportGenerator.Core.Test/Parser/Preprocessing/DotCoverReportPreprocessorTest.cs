using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for DotCoverReportPreprocessor and is intended
    /// to contain all DotCoverReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class DotCoverReportPreprocessorTest
    {
        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "dotCover.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_MoveStartupCodeElementsToParentType()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeClasses = report.Root
                .Elements("Assembly")
                .Elements("Namespace")
                .Where(c => c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .Where(t => t.Attribute("Name").Value.StartsWith("$Module", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .ToArray();

            Assert.Equal(14, startupCodeClasses.Length);

            new DotCoverReportPreprocessor().Execute(report);

            var updatedStartupCodeClasses = report.Root
                .Elements("Assembly")
                .Elements("Namespace")
                .Where(c => c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .Where(t => t.Attribute("Name").Value.StartsWith("$Module", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .ToArray();

            Assert.Empty(updatedStartupCodeClasses);

            for (int i = 3; i < 7; i++)
            {
                Assert.StartsWith("MouseBehavior", startupCodeClasses[i].Parent.Attribute("Name").Value);
            }

            for (int i = 8; i < 13; i++)
            {
                Assert.StartsWith("TestMouseBehavior", startupCodeClasses[i].Parent.Attribute("Name").Value);
            }
        }
    }
}
