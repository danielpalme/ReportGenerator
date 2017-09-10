using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGeneratorTest.Reporting
{
    /// <summary>
    /// This is a test class for DefaultFilter and is intended
    /// to contain all DefaultFilter Unit Tests
    /// </summary>
    [TestClass]
    public class DefaultFilterTest
    {
        [TestMethod]
        public void NoFilter_AnyElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new string[] { });

            Assert.IsTrue(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
        }

        [TestMethod]
        public void OnlyIncludes_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test" });

            Assert.IsTrue(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
        }

        [TestMethod]
        public void OnlyIncludes_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test" });

            Assert.IsFalse(filter.IsElementIncludedInReport("Test123"), "Element is expected to be excluded.");
        }

        [TestMethod]
        public void OnlyIncludesWithWildcards_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*" });

            Assert.IsTrue(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.IsTrue(filter.IsElementIncludedInReport("Test123"), "Element is expected to be included.");
        }

        [TestMethod]
        public void OnlyIncludesWithWildcards_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*" });

            Assert.IsFalse(filter.IsElementIncludedInReport("PrefixTest"), "Element is expected to be included.");
            Assert.IsFalse(filter.IsElementIncludedInReport("PrefixTest123"), "Element is expected to be included.");
        }

        [TestMethod]
        public void IncludesAndExcludes_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test", "-SomeExclude" });

            Assert.IsTrue(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
        }

        [TestMethod]
        public void IncludesAndExcludes_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test", "-Test" });

            Assert.IsFalse(filter.IsElementIncludedInReport("Test"), "Element is expected to be excluded.");
        }

        [TestMethod]
        public void IncludesAndExcludesWithWildcards_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*", "-SomeExclude*" });

            Assert.IsTrue(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.IsTrue(filter.IsElementIncludedInReport("Test123"), "Element is expected to be included.");
        }

        [TestMethod]
        public void IncludesAndExcludesWithWildcards_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*", "-Tes*" });

            Assert.IsFalse(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.IsFalse(filter.IsElementIncludedInReport("PrefixTest123"), "Element is expected to be included.");
        }
    }
}
