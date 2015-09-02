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
    /// This is a test class for ReportConfigurationBuilder and is intended
    /// to contain all ReportConfigurationBuilder Unit Tests
    /// </summary>
    [TestClass]
    public class ReportConfigurationBuilderTest
    {
        private static readonly string ReportPath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private ReportConfigurationBuilder reportConfigurationBuilder;

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
            this.reportConfigurationBuilder = new ReportConfigurationBuilder(new Mock<IReportBuilderFactory>().Object);
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
        public void InitWithLegacyArguments_AllPropertiesApplied()
        {
            string[] legacyArguments = new string[]
            {
                ReportPath,
                "C:\\temp",
                "Latex"
            };

            this.configuration = this.reportConfigurationBuilder.Create(legacyArguments);

            Assert.IsTrue(this.configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.AreEqual("C:\\temp", this.configuration.TargetDirectory, "Wrong target directory applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.IsFalse(this.configuration.SourceDirectories.Any(), "Source directories should be empty.");
            Assert.IsFalse(this.configuration.AssemblyFilters.Any(), "AssemblyFilters should be empty.");
            Assert.IsFalse(this.configuration.ClassFilters.Any(), "ClassFilters should be empty.");
        }

        [TestMethod]
        public void InitWithNamedArguments_OldFilters_AllPropertiesApplied()
        {
            string[] namedArguments = new string[]
            {
                "-reports:" + ReportPath,
                "-targetdir:C:\\temp",
                "-reporttype:Latex",
                "-sourcedirs:C:\\temp\\source;C:\\temp\\source2",
                "-filters:+Test;-Test",
                "-verbosity:" + VerbosityLevel.Info.ToString()
            };

            this.configuration = this.reportConfigurationBuilder.Create(namedArguments);

            Assert.IsTrue(this.configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.AreEqual("C:\\temp", this.configuration.TargetDirectory, "Wrong target directory applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.IsTrue(this.configuration.SourceDirectories.Contains("C:\\temp\\source"), "Directory does not exist in Source directories.");
            Assert.IsTrue(this.configuration.SourceDirectories.Contains("C:\\temp\\source2"), "Directory does not exist in Source directories.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("+Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("-Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.AreEqual(VerbosityLevel.Info, this.configuration.VerbosityLevel, "Wrong verbosity level applied.");
        }

        [TestMethod]
        public void InitWithNamedArguments_NewFilters_AllPropertiesApplied()
        {
            string[] namedArguments = new string[]
            {
                "-reports:" + ReportPath,
                "-targetdir:C:\\temp",
                "-reporttype:Latex",
                "-sourcedirs:C:\\temp\\source;C:\\temp\\source2",
                "-assemblyfilters:+Test;-Test",
                "-classfilters:+Test2;-Test2",
                "-verbosity:" + VerbosityLevel.Info.ToString()
            };

            this.configuration = this.reportConfigurationBuilder.Create(namedArguments);

            Assert.IsTrue(this.configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.AreEqual("C:\\temp", this.configuration.TargetDirectory, "Wrong target directory applied.");
            Assert.IsTrue(this.configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.IsTrue(this.configuration.SourceDirectories.Contains("C:\\temp\\source"), "Directory does not exist in Source directories.");
            Assert.IsTrue(this.configuration.SourceDirectories.Contains("C:\\temp\\source2"), "Directory does not exist in Source directories.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("+Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.ClassFilters.Contains("+Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.AssemblyFilters.Contains("-Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.IsTrue(this.configuration.ClassFilters.Contains("-Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.AreEqual(VerbosityLevel.Info, this.configuration.VerbosityLevel, "Wrong verbosity level applied.");
        }
    }
}
