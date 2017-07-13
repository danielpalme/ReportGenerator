using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Palmmedia.ReportGenerator;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGeneratorTest
{
    /// <summary>
    /// This is a test class for ReportConfiguration and is intended
    /// to contain all ReportConfiguration Unit Tests
    /// </summary>
    [TestClass]
    public class ReportConfigurationTest
    {
        private static readonly string ReportPath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private Mock<IReportBuilderFactory> reportBuilderFactoryMock;

        private ReportConfiguration configuration;

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup]
        // public static void MyClassCleanup()
        // {
        // }

        // Use TestInitialize to run code before running each test
        [TestInitialize]
        public void MyTestInitialize()
        {
            this.reportBuilderFactoryMock = new Mock<IReportBuilderFactory>();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            Assert.IsNotNull(this.configuration.ReportFiles);
            Assert.IsNotNull(this.configuration.SourceDirectories);
            Assert.IsNotNull(this.configuration.AssemblyFilters);
            Assert.IsNotNull(this.configuration.ClassFilters);
        }

        #endregion

        [TestMethod]
        public void InitByConstructor_AllDefaultValuesApplied()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp",
                "C:\\temp\\historic",
                new string[] { },
                new string[] { },
                new string[] { },
                new string[] { },
                new string[] { },
                string.Empty);

            Assert.IsTrue(this.configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.AreEqual("C:\\temp", this.configuration.TargetDirectory, "Wrong target directory applied.");
            Assert.AreEqual("C:\\temp\\historic", this.configuration.HistoryDirectory, "Wrong target directory applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Html"), "Wrong report type applied.");
            Assert.AreEqual(0, this.configuration.SourceDirectories.Count(), "Wrong number of source directories.");
            Assert.AreEqual(0, this.configuration.AssemblyFilters.Count(), "Wrong number of AssemblyFilters.");
            Assert.AreEqual(0, this.configuration.ClassFilters.Count(), "Wrong number of ClassFilters.");
            Assert.AreEqual(VerbosityLevel.Verbose, this.configuration.VerbosityLevel, "Wrong verbosity level applied.");
        }

        [TestMethod]
        public void InitByConstructor_AllPropertiesApplied()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp",
                null,
                new[] { "Latex", "Xml", "Html" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new[] { "+Test3", "-Test3" },
                VerbosityLevel.Info.ToString());

            Assert.IsTrue(this.configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.AreEqual("C:\\temp", this.configuration.TargetDirectory, "Wrong target directory applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Xml"), "Wrong report type applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Html"), "Wrong report type applied.");
            Assert.IsTrue(this.configuration.SourceDirectories.Contains(FileManager.GetCSharpCodeDirectory()), "Directory does not exist in Source directories.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("+Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("-Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.ClassFilters.Contains("+Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.ClassFilters.Contains("-Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.FileFilters.Contains("+Test3"), "FileFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.FileFilters.Contains("-Test3"), "FileFilters does not exist in ReportFiles.");
            Assert.AreEqual(VerbosityLevel.Info, this.configuration.VerbosityLevel, "Wrong verbosity level applied.");
        }

        [TestMethod]
        public void Validate_AllPropertiesApplied_ValidationPasses()
        {
            this.reportBuilderFactoryMock
                .Setup(r => r.GetAvailableReportTypes())
                .Returns(new[] { "Latex", "Xml", "Html", "Something" });

            this.InitByConstructor_AllPropertiesApplied();

            Assert.IsTrue(this.configuration.Validate(), "Validation should pass.");
        }

        [TestMethod]
        public void Validate_NoReport_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new string[] { },
                "C:\\temp",
                null,
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_NonExistingReport_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { "123.xml" },
                "C:\\temp",
                null,
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_NoTargetDirectory_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                string.Empty,
                null,
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_InvalidTargetDirectory_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp:?$",
                null,
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_InvalidHistoryDirectory_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp",
                "C:\\temp:?$",
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_InvalidReportType_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp",
                null,
                new[] { "DoesNotExist" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_NotExistingSourceDirectory_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                "C:\\temp",
                null,
                new[] { "Latex" },
                new[] { Path.Combine(FileManager.GetCSharpCodeDirectory(), "123456") },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }

        [TestMethod]
        public void Validate_InvalidFilter_ValidationFails()
        {
            this.configuration = new ReportConfiguration(
                this.reportBuilderFactoryMock.Object,
                new[] { ReportPath },
                @"C:\\temp",
                null,
                new[] { "Latex" },
                new[] { FileManager.GetCSharpCodeDirectory() },
                new[] { "Test" },
                new[] { "Test2" },
                new string[] { },
                VerbosityLevel.Info.ToString());

            Assert.IsFalse(this.configuration.Validate(), "Validation should fail.");
        }
    }
}
