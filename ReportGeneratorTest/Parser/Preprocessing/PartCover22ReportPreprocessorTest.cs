using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing
{
    /// <summary>
    /// This is a test class for PartCover22ReportPreprocessor and is intended
    /// to contain all PartCover22ReportPreprocessor Unit Tests
    /// </summary>
    [TestClass]
    public class PartCover22ReportPreprocessorTest
    {
        private static readonly string FilePath = Path.Combine(FileManager.GetCSharpReportDirectory(), "Partcover2.2.xml");

        /// <summary>
        /// A test for Execute
        /// </summary>
        [TestMethod]
        public void Execute_SequencePointsOfAutoPropertiesAdded()
        {
            XDocument report = XDocument.Load(FilePath);

            var classSearcherFactory = new ClassSearcherFactory();
            new PartCover22ReportPreprocessor(report, classSearcherFactory, new ClassSearcher(string.Empty)).Execute();

            Assert.AreEqual(7, report.Root.Elements("file").Count(), "Wrong number of total files.");

            var gettersAndSetters = report.Root.Elements("type")
                .Single(c => c.Attribute("name").Value == "Test.TestClass2")
                .Elements("method")
                .Where(m => m.Attribute("name").Value.StartsWith("get_") || m.Attribute("name").Value.StartsWith("set_"))
                .Elements("code")
                .Select(c => c.Element("pt"));

            Assert.IsTrue(gettersAndSetters.Any());

            foreach (var getterOrSetter in gettersAndSetters)
            {
                Assert.IsTrue(getterOrSetter.Attribute("fid") != null);
                Assert.IsTrue(getterOrSetter.Attribute("sl") != null);
            }
        }
    }
}
