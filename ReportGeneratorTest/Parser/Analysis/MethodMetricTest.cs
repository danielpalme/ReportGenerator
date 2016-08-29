using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGeneratorTest.Parser.Analysis
{
    /// <summary>
    /// This is a test class for MethodMetric and is intended
    /// to contain all MethodMetric Unit Tests
    /// </summary>
    [TestClass]
    public class MethodMetricTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [TestMethod]
        public void Constructor()
        {
            MethodMetric sut = new MethodMetric("Test");

            Assert.AreEqual("Test", sut.Name, "Not equal");
        }

        /// <summary>
        /// A test for AddMetric
        /// </summary>
        [TestMethod]
        public void AddMetric_AddSingleMetric_MetricIsStored()
        {
            MethodMetric sut = new MethodMetric("Test");
            var metric = new Metric("Metric1", null, 10);

            sut.AddMetric(metric);

            Assert.AreEqual(metric, sut.Metrics.First(), "Not equal");
            Assert.AreEqual(1, sut.Metrics.Count(), "Wrong number of metrics");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
        public void Merge_MergeMethodMetric_MetricsAreStored()
        {
            var metric1 = new Metric("Metric1", null, 10);
            var metric2 = new Metric("Metric1", null, 15);
            var metric3 = new Metric("Metric2", null, 20);

            MethodMetric sut = new MethodMetric("Test", new[] { metric1 });
            var methodMetricToMerge = new MethodMetric("Test", new[] { metric2, metric3 });

            sut.Merge(methodMetricToMerge);

            Assert.AreEqual(2, sut.Metrics.Count(), "Wrong number of metrics");
            Assert.AreEqual(metric1, sut.Metrics.First(), "Not equal");
            Assert.AreEqual(15, sut.Metrics.First().Value, "Not equal");
            Assert.AreEqual(metric3, sut.Metrics.ElementAt(1), "Not equal");
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [TestMethod]
        public void Equals()
        {
            var target1 = new MethodMetric("Test");
            var target2 = new MethodMetric("Test");
            var target3 = new MethodMetric("Other");

            Assert.IsTrue(target1.Equals(target2), "Objects are not equal");
            Assert.IsFalse(target1.Equals(target3), "Objects are equal");
            Assert.IsFalse(target1.Equals(null), "Objects are equal");
            Assert.IsFalse(target1.Equals(new object()), "Objects are equal");
        }
    }
}
