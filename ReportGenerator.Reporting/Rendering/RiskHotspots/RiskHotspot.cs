
namespace Palmmedia.ReportGenerator.Reporting.Rendering.RiskHotspots
{
    internal class RiskHotspot
    {
        public string AssemblyShortName { get; set; }
        public string ClassName { get; set; }
        public string ClassNameShort => GetClassNameShort(ClassName);
        public string MethodNameShort { get; set; }
        public decimal Complexity { get; set; }
        public decimal Coverage { get; set; }
        public decimal BranchCoverage { get; set; }
        public decimal CrapScore { get; set; }

        private string GetClassNameShort(string className)
        {
            var lastDotPosition = className.LastIndexOf('.');
            return className.Substring(lastDotPosition + 1);
        }
    }
}
