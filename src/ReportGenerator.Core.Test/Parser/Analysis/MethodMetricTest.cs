using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Analysis
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
            MethodMetric sut = new MethodMetric("Test", "Test2", Enumerable.Empty<Metric>());

            Assert.Equal("Test", sut.FullName);
            Assert.Equal("Test2", sut.ShortName);
        }

        /// <summary>
        /// A test for AddMetric
        /// </summary>
        [Fact]
        public void AddMetric_AddSingleMetric_MetricIsStored()
        {
            MethodMetric sut = new MethodMetric("Test", "Test", Enumerable.Empty<Metric>());
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
            var metric1_1 = new Metric("Metric1", null, MetricType.CodeQuality, 10);
            var metric1_2 = new Metric("Metric1", null, MetricType.CodeQuality, 15);
            var metric2 = new Metric("Metric2", null, MetricType.CodeQuality, 20);
            var metric3_1 = new Metric("Metric3", null, MetricType.CodeQuality, null);
            var metric3_2 = new Metric("Metric3", null, MetricType.CodeQuality, null);
            var metric4_1 = new Metric("Metric4", null, MetricType.CodeQuality, null);
            var metric4_2 = new Metric("Metric4", null, MetricType.CodeQuality, 10);
            var metric5_1 = new Metric("Metric5", null, MetricType.CodeQuality, 30);
            var metric5_2 = new Metric("Metric5", null, MetricType.CodeQuality, null);
            var metric6_1 = new Metric("Metric6", null, MetricType.CodeQuality, 50, MetricMergeOrder.LowerIsBetter);
            var metric6_2 = new Metric("Metric6", null, MetricType.CodeQuality, 40, MetricMergeOrder.LowerIsBetter);

            MethodMetric sut = new MethodMetric("Test", "Test", new[] { metric1_1, metric3_1, metric4_1, metric5_1, metric6_1 });
            var methodMetricToMerge = new MethodMetric("Test", "Test", new[] { metric1_2, metric2, metric3_2, metric4_2, metric5_2, metric6_2 });

            sut.Merge(methodMetricToMerge);

            Assert.Equal(6, sut.Metrics.Count());
            Assert.Equal(metric1_1, sut.Metrics.First());
            Assert.Equal(15, sut.Metrics.First().Value);

            var metric = sut.Metrics.Single(m => m.Name == "Metric2");
            Assert.Equal(metric2, metric);
            Assert.Equal(20, metric.Value);

            metric = sut.Metrics.Single(m => m.Name == "Metric3");
            Assert.Equal(metric3_1, metric);
            Assert.Null(metric.Value);

            metric = sut.Metrics.Single(m => m.Name == "Metric4");
            Assert.Equal(metric4_1, metric);
            Assert.Equal(10, metric.Value);

            metric = sut.Metrics.Single(m => m.Name == "Metric5");
            Assert.Equal(metric5_1, metric);
            Assert.Equal(30, metric.Value);

            metric = sut.Metrics.Single(m => m.Name == "Metric6");
            Assert.Equal(metric6_1, metric);
            Assert.Equal(40, metric.Value);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void MethodMetric_Equals()
        {
            var target1 = new MethodMetric("Test", "Test", Enumerable.Empty<Metric>());
            var target2 = new MethodMetric("Test", "Test", Enumerable.Empty<Metric>());
            var target3 = new MethodMetric("Other", "Other", Enumerable.Empty<Metric>());
            var target4 = new MethodMetric("Test", "Test", Enumerable.Empty<Metric>())
            { Line = 3 };

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(target4), "Objects are equal");
            Assert.True(target4.Equals(target4), "Objects are not equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }
    }
}
