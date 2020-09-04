using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Analysis
{
    /// <summary>
    /// This is a test class for Branch and is intended
    /// to contain all Branch Unit Tests
    /// </summary>
    public class BranchTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Fact]
        public void Constructor()
        {
            var sut = new Branch(10, "Test");

            Assert.Equal(10, sut.BranchVisits);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void Branch_Equals()
        {
            var target1 = new Branch(10, "Test");
            var target2 = new Branch(11, "Test");
            var target3 = new Branch(10, "Test123");

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }
    }
}
