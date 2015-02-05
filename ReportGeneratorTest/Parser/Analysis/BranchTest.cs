using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGeneratorTest.Parser.Analysis
{
    /// <summary>
    /// This is a test class for Branch and is intended
    /// to contain all Branch Unit Tests
    /// </summary>
    [TestClass]
    public class BranchTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [TestMethod]
        public void Constructor()
        {
            var sut = new Branch(10, "Test");

            Assert.AreEqual(10, sut.BranchVisits, "Not equal");
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [TestMethod]
        public void Equals()
        {
            var target1 = new Branch(10, "Test");
            var target2 = new Branch(11, "Test");
            var target3 = new Branch(10, "Test123");

            Assert.IsTrue(target1.Equals(target2), "Objects are not equal");
            Assert.IsFalse(target1.Equals(target3), "Objects are equal");
            Assert.IsFalse(target1.Equals(null), "Objects are equal");
            Assert.IsFalse(target1.Equals(new object()), "Objects are equal");
        }
    }
}
