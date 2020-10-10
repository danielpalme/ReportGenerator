using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Filtering
{
    /// <summary>
    /// This is a test class for DefaultFilter and is intended
    /// to contain all DefaultFilter Unit Tests
    /// </summary>
    public class DefaultFilterTest
    {
        [Fact]
        public void NoFilter_AnyElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new string[] { });

            Assert.True(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.False(filter.HasCustomFilters);
        }

        [Fact]
        public void OnlyIncludes_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test" });

            Assert.True(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void OnlyIncludes_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test" });

            Assert.False(filter.IsElementIncludedInReport("Test123"), "Element is expected to be excluded.");
            Assert.False(filter.IsElementIncludedInReport("test123"), "Element is expected to be excluded.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void OnlyIncludesWithWildcards_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*" });

            Assert.True(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("Test123"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test123"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void OnlyIncludesWithWildcards_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*" });

            Assert.False(filter.IsElementIncludedInReport("PrefixTest"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("prefixtest"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("PrefixTest123"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("prefixtest123"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void IncludesAndExcludes_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test", "-SomeExclude" });

            Assert.True(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void IncludesAndExcludes_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test", "-Test" });

            Assert.False(filter.IsElementIncludedInReport("Test"), "Element is expected to be excluded.");
            Assert.False(filter.IsElementIncludedInReport("test"), "Element is expected to be excluded.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void IncludesAndExcludesWithWildcards_MatchingElement_ElementIsAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*", "-SomeExclude*" });

            Assert.True(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("Test123"), "Element is expected to be included.");
            Assert.True(filter.IsElementIncludedInReport("test123"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }

        [Fact]
        public void IncludesAndExcludesWithWildcards_NotMatchingElement_ElementIsNotAccepted()
        {
            IFilter filter = new DefaultFilter(new[] { "+Test*", "-Tes*" });

            Assert.False(filter.IsElementIncludedInReport("Test"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("test"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("PrefixTest123"), "Element is expected to be included.");
            Assert.False(filter.IsElementIncludedInReport("prefixtest123"), "Element is expected to be included.");
            Assert.True(filter.HasCustomFilters);
        }
    }
}
