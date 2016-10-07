using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGeneratorTest.Parser.Analysis
{
    /// <summary>
    /// This is a test class for ClassTest and is intended
    /// to contain all ClassTest Unit Tests
    /// </summary>
    [TestClass]
    public class ClassTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [TestMethod]
        public void Constructor()
        {
            Assembly assembly = new Assembly("C:\\test\\TestAssembly.dll");
            string classname = "TestClass";

            var sut = new Class(classname, assembly);

            Assert.AreEqual(assembly, sut.Assembly, "Not equal");
            Assert.AreEqual(classname, sut.Name, "Not equal");
        }

        /// <summary>
        /// A test for AddFile
        /// </summary>
        [TestMethod]
        public void AddFile_AddSingleFile_FileIsStored()
        {
            var assembly = new Assembly("C:\\test\\TestAssembly.dll");
            var sut = new Class("Test", assembly);
            var file = new CodeFile("C:\\temp\\Program.cs", new int[0], new LineVisitStatus[0]);
            sut.AddFile(file);

            Assert.AreEqual(file, sut.Files.First(), "Not equal");
            Assert.AreEqual(1, sut.Files.Count(), "Wrong number of classes");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
        public void Merge_MergeClassWithOneFileAndOneMethodMetric_FileIsStored()
        {
            var assembly = new Assembly("C:\\test\\TestAssembly.dll");
            var sut = new Class("Test", assembly);
            var classToMerge = new Class("Test", assembly);
            var file = new CodeFile("C:\\temp\\Program.cs", new int[0], new LineVisitStatus[0]);
            var methodMetric = new MethodMetric("Test");
            classToMerge.AddFile(file);
            classToMerge.AddMethodMetric(methodMetric);
            sut.Merge(classToMerge);

            Assert.AreEqual(file, sut.Files.First(), "Not equal");
            Assert.AreEqual(1, sut.Files.Count(), "Wrong number of classes");
            Assert.AreEqual(methodMetric, sut.MethodMetrics.First(), "Not equal");
            Assert.AreEqual(1, sut.MethodMetrics.Count(), "Wrong number of method metrics");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
        public void Merge_MergeClassWithCoverageQuota_FileIsStored()
        {
            var assembly = new Assembly("C:\\test\\TestAssembly.dll");
            var sut = new Class("Test", assembly);
            var classToMerge = new Class("Test", assembly)
            {
                CoverageQuota = 15
            };

            sut.Merge(classToMerge);

            Assert.AreEqual(15, sut.CoverageQuota);

            classToMerge = new Class("Test", assembly)
            {
                CoverageQuota = 20
            };

            sut.Merge(classToMerge);

            Assert.AreEqual(20, sut.CoverageQuota);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [TestMethod]
        public void Equals()
        {
            Assembly assembly = new Assembly("C:\\test\\TestAssembly.dll");
            string classname = "TestClass";

            var target1 = new Class(classname, assembly);
            var target2 = new Class(classname, assembly);
            var target3 = new Class(classname + "123", assembly);

            Assert.IsTrue(target1.Equals(target2), "Objects are not equal");
            Assert.IsFalse(target1.Equals(target3), "Objects are equal");
            Assert.IsFalse(target1.Equals(null), "Objects are equal");
            Assert.IsFalse(target1.Equals(new object()), "Objects are equal");
        }
    }
}
