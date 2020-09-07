using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for OpenCoverReportPreprocessor and is intended
    /// to contain all OpenCoverReportPreprocessor Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class OpenCoverReportPreprocessorTest
    {
        private static readonly string CSharpFilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private static readonly string FSharpFilePath = Path.Combine(FileManager.GetFSharpReportDirectory(), "OpenCover.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_SequencePointsOfAutoPropertiesAdded()
        {
            XDocument report = XDocument.Load(CSharpFilePath);

            new OpenCoverReportPreprocessor().Execute(report);

            Assert.Equal(15, report.Descendants("File").Count());

            var gettersAndSetters = report.Descendants("Class")
                .Single(c => c.Element("FullName") != null && c.Element("FullName").Value == "Test.TestClass2")
                .Elements("Methods")
                .Elements("Method")
                .Where(m => m.Attribute("isGetter").Value == "true" || m.Attribute("isSetter").Value == "true");

            foreach (var getterOrSetter in gettersAndSetters)
            {
                Assert.True(getterOrSetter.Element("FileRef") != null);
                Assert.True(getterOrSetter.Element("SequencePoints") != null);

                var sequencePoints = getterOrSetter.Element("SequencePoints").Elements("SequencePoint");
                Assert.Single(sequencePoints);
                Assert.Equal(getterOrSetter.Element("MethodPoint").Attribute("vc").Value, sequencePoints.First().Attribute("vc").Value);
            }
        }

        /// <summary>
        /// A test for Execute
        /// </summary>
        [Fact]
        public void Execute_ClassNameAddedToStartupCodeElements()
        {
            XDocument report = XDocument.Load(FSharpFilePath);

            var startupCodeClasses = report.Root
                .Elements("Modules")
                .Elements("Module")
                .Elements("Classes")
                .Elements("Class")
                .Where(c => c.Element("FullName").Value.StartsWith("<StartupCode$"))
                .ToArray();

            Assert.Equal(17, startupCodeClasses.Length);

            new OpenCoverReportPreprocessor().Execute(report);

            var updatedStartupCodeClasses = report.Root
                .Elements("Modules")
                .Elements("Module")
                .Elements("Classes")
                .Elements("Class")
                .Where(c => c.Element("FullName").Value.StartsWith("<StartupCode$"))
                .ToArray();

            Assert.Equal(3, updatedStartupCodeClasses.Length);

            for (int i = 2; i < 9; i++)
            {
                Assert.StartsWith("ViewModels.MouseBehavior/", startupCodeClasses[i].Element("FullName").Value);
            }

            for (int i = 9; i < 16; i++)
            {
                Assert.StartsWith("ViewModels.TestMouseBehavior/", startupCodeClasses[i].Element("FullName").Value);
            }
        }
    }
}
