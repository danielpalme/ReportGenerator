using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser.Analysis
{
    /// <summary>
    /// This is a test class for MethodMetric and is intended
    /// to contain all MethodMetric Unit Tests
    /// </summary>
    public class MethodMetricTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Fact]
        public void Constructor()
        {
            MethodMetric sut = new MethodMetric("Test");

            Assert.Equal("Test", sut.Name);
        }

        /// <summary>
        /// A test for AddMetric
        /// </summary>
        [Fact]
        public void AddMetric_AddSingleMetric_MetricIsStored()
        {
            MethodMetric sut = new MethodMetric("Test");
            var metric = new Metric("Metric1", null, MetricType.CodeQuality, 10);

            sut.AddMetric(metric);

            Assert.Equal(metric, sut.Metrics.First());
            Assert.Single(sut.Metrics);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_MergeMethodMetric_MetricsAreStored()
        {
            var metric1 = new Metric("Metric1", null, MetricType.CodeQuality, 10);
            var metric2 = new Metric("Metric1", null, MetricType.CodeQuality, 15);
            var metric3 = new Metric("Metric2", null, MetricType.CodeQuality, 20);

            MethodMetric sut = new MethodMetric("Test", new[] { metric1 });
            var methodMetricToMerge = new MethodMetric("Test", new[] { metric2, metric3 });

            sut.Merge(methodMetricToMerge);

            Assert.Equal(2, sut.Metrics.Count());
            Assert.Equal(metric1, sut.Metrics.First());
            Assert.Equal(15, sut.Metrics.First().Value);
            Assert.Equal(metric3, sut.Metrics.ElementAt(1));
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void MethodMetric_Equals()
        {
            var target1 = new MethodMetric("Test");
            var target2 = new MethodMetric("Test");
            var target3 = new MethodMetric("Other");
            var target4 = new MethodMetric("Test") { Line = 3 };

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(target4), "Objects are equal");
            Assert.True(target4.Equals(target4), "Objects are not equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }
    }
}
