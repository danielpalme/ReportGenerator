
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Reporting.Rendering.RiskHotspots;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Properties;

namespace ReportGenerator.Reporting.Test.Rendering.RiskHotspots
{
    [TestClass]
    public class RiskHotspotsAnalysisTest
    {
        [TestMethod]
        public void NoHotspots_WhenNoAssemblies()
        {
            // arrage
            var assemblies = new List<Assembly>();

            // act
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies);

            // assert
            Assert.AreEqual(0, hotspots.Count());
        }

        [TestMethod]
        public void OneHotspot_ForEveryClass()
        {
            // arrage
            var assembly1 = new Assembly("assembly1");
            var assembly2 = new Assembly("assembly2");
            var @class1A = new Class("class1A", assembly1);
            var @class1B = new Class("class1B", assembly1);
            var @class2A = new Class("class2A", assembly2);
            var @class2B = new Class("class2B", assembly1);
            assembly1.AddClass(@class1A);
            assembly1.AddClass(@class1B);
            assembly2.AddClass(@class2A);
            assembly2.AddClass(@class2B);
            var method1A1 = new MethodMetric("method1A1", new List<Metric> {new Metric("dummyMetric", null, 1)});
            var method1A2 = new MethodMetric("method1A2", new List<Metric> { new Metric("dummyMetric", null, 1) });
            @class1A.AddMethodMetric(method1A1);
            @class1A.AddMethodMetric(method1A2);
            var method1B1 = new MethodMetric("method1B1", new List<Metric> { new Metric("dummyMetric", null, 1) });
            var method1B2 = new MethodMetric("method1B2", new List<Metric> { new Metric("dummyMetric", null, 1) });
            @class1B.AddMethodMetric(method1B1);
            @class1B.AddMethodMetric(method1B2);
            var method2A1 = new MethodMetric("method2A1", new List<Metric> { new Metric("dummyMetric", null, 1) });
            @class2A.AddMethodMetric(method2A1);
            var method2B1 = new MethodMetric("method2B1", new List<Metric> { new Metric("dummyMetric", null, 1) });
            @class2B.AddMethodMetric(method2B1);

            var assemblies = new List<Assembly> {assembly1, assembly2};

            // act
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies);

            // assert
            Assert.AreEqual(4, hotspots.Count());
        }

        [TestMethod]
        public void OnlyWorstHotspot_Taken_FromEveryClass()
        {
            // arrage
            var assembly = new Assembly("assembly");
            var classA = new Class("classA", assembly);
            assembly.AddClass(classA);
            var methodA1 = new MethodMetric("methodA1", new List<Metric> { new Metric(ReportResources.CrapScore, null, 2) });
            var methodA2 = new MethodMetric("methodA2", new List<Metric> { new Metric(ReportResources.CrapScore, null, 22) });
            classA.AddMethodMetric(methodA1);
            classA.AddMethodMetric(methodA2);
            var classB = new Class("classB", assembly);
            assembly.AddClass(classB);
            var methodB1 = new MethodMetric("methodB1", new List<Metric> { new Metric(ReportResources.CrapScore, null, 11) });
            var methodB2 = new MethodMetric("methodB2", new List<Metric> { new Metric(ReportResources.CrapScore, null, 1) });
            classB.AddMethodMetric(methodB1);
            classB.AddMethodMetric(methodB2);
            var assemblies = new List<Assembly> {assembly};

            // act
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies);

            // assert
            var hotspotsList = hotspots.ToList();
            Assert.AreEqual("assembly", hotspotsList[0].AssemblyShortName);
            Assert.AreEqual("assembly", hotspotsList[1].AssemblyShortName);
            Assert.AreEqual("classA", hotspotsList[0].ClassNameShort);
            Assert.AreEqual("classB", hotspotsList[1].ClassNameShort);
            Assert.AreEqual("methodA2", hotspotsList[0].MethodNameShort);
            Assert.AreEqual("methodB1", hotspotsList[1].MethodNameShort);
            Assert.AreEqual(22, hotspotsList[0].CrapScore);
            Assert.AreEqual(11, hotspotsList[1].CrapScore);
        }

        [TestMethod]
        public void Hotspot_Metrics()
        {
            // arrage
            var assembly = new Assembly("assembly");
            var @class = new Class("class", assembly);
            assembly.AddClass(@class);
            var method = new MethodMetric("method");
            method.AddMetric(new Metric(ReportResources.CyclomaticComplexity, null, 1));
            method.AddMetric(new Metric(ReportResources.SequenceCoverage, null, 2));
            method.AddMetric(new Metric(ReportResources.BranchCoverage, null, 3));
            method.AddMetric(new Metric(ReportResources.CrapScore, null, 4));
            @class.AddMethodMetric(method);
            var assemblies = new List<Assembly> { assembly };

            // act
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies);

            // assert
            var hotspot = hotspots.First();
            Assert.AreEqual(1, hotspot.Complexity);
            Assert.AreEqual(2, hotspot.Coverage);
            Assert.AreEqual(3, hotspot.BranchCoverage);
            Assert.AreEqual(4, hotspot.CrapScore);
        }

        [TestMethod]
        public void Hotspots_SortedBy_CrapScore()
        {
            // arrage
            var assembly = new Assembly("assembly");
            var class1 = new Class("class1", assembly);
            var class2 = new Class("class2", assembly);
            var class3 = new Class("class3", assembly);
            assembly.AddClass(class1);
            assembly.AddClass(class2);
            assembly.AddClass(class3);
            var method1 = new MethodMetric("method1", new List<Metric> { new Metric(ReportResources.CrapScore, null, 10) });
            var method2 = new MethodMetric("method2", new List<Metric> { new Metric(ReportResources.CrapScore, null, 30) });
            var method3 = new MethodMetric("method3", new List<Metric> { new Metric(ReportResources.CrapScore, null, 20) });
            class1.AddMethodMetric(method1);
            class2.AddMethodMetric(method2);
            class3.AddMethodMetric(method3);
            var assemblies = new List<Assembly> {assembly};

            // act
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies);

            // assert
            var hotspotsList = hotspots.ToList();
            Assert.AreEqual("method2", hotspotsList[0].MethodNameShort);
            Assert.AreEqual("method3", hotspotsList[1].MethodNameShort);
            Assert.AreEqual("method1", hotspotsList[2].MethodNameShort);
            Assert.AreEqual(30, hotspotsList[0].CrapScore);
            Assert.AreEqual(20, hotspotsList[1].CrapScore);
            Assert.AreEqual(10, hotspotsList[2].CrapScore);
        }

        [TestMethod]
        public void Hotspots_Count_Limited_ToTopThree()
        {
            // arrage
            var assembly = new Assembly("assembly");
            var class1 = new Class("class1", assembly);
            var class2 = new Class("class2", assembly);
            var class3 = new Class("class3", assembly);
            var class4 = new Class("class4", assembly);
            var class5 = new Class("class5", assembly);
            assembly.AddClass(class1);
            assembly.AddClass(class2);
            assembly.AddClass(class3);
            assembly.AddClass(class4);
            assembly.AddClass(class5);
            var method1 = new MethodMetric("method1", new List<Metric> { new Metric(ReportResources.CrapScore, null, 40) });
            var method2 = new MethodMetric("method2", new List<Metric> { new Metric(ReportResources.CrapScore, null, 30) });
            var method3 = new MethodMetric("method3", new List<Metric> { new Metric(ReportResources.CrapScore, null, 20) });
            var method4 = new MethodMetric("method4", new List<Metric> { new Metric(ReportResources.CrapScore, null, 10) });
            var method5 = new MethodMetric("method5", new List<Metric> { new Metric(ReportResources.CrapScore, null, 50) });
            class1.AddMethodMetric(method1);
            class2.AddMethodMetric(method2);
            class3.AddMethodMetric(method3);
            class4.AddMethodMetric(method4);
            class5.AddMethodMetric(method5);
            var assemblies = new List<Assembly> {assembly};

            // act
            const int maxHotspots = 3;
            var hotspots = RiskHotspotsAnalysis.DetectHotspots(assemblies, maxHotspots);

            // assert
            var hotspotsList = hotspots.ToList();
            Assert.AreEqual(maxHotspots, hotspotsList.Count);
            Assert.AreEqual("method5", hotspotsList[0].MethodNameShort);
            Assert.AreEqual("method1", hotspotsList[1].MethodNameShort);
            Assert.AreEqual("method2", hotspotsList[2].MethodNameShort);
            Assert.AreEqual(50, hotspotsList[0].CrapScore);
            Assert.AreEqual(40, hotspotsList[1].CrapScore);
            Assert.AreEqual(30, hotspotsList[2].CrapScore);
        }
    }
}
