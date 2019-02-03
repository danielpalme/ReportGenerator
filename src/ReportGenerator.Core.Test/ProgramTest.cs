using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Reporting;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    [Collection("FileManager")]
    public class ProgramTest : IEnumerable<object[]>
    {
        [Theory]
        [ClassData(typeof(ProgramTest))]
        public void CreateReport(string reportType)
        {
            string targetdir = $@"..\..\..\..\target\samplereports\{reportType}";
            string historydir = $@"{targetdir}\history";

            if (Directory.Exists(historydir))
            {
                Directory.Delete(historydir, true);
            }

            Program.Main(new[]
            {
                $"-reports:{Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCoverWithTrackedMethods.xml")}",
                $"-reporttypes:{reportType}",
                $"-targetdir:{targetdir}",
                $"-historydir:{historydir}",
                $"-verbosity:Error"
            });
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var reportBuilderFactory = new ReportBuilderFactory(new ReflectionPluginLoader(new List<string>()));

            foreach (var reportType in reportBuilderFactory.GetAvailableReportTypes())
            {
                yield return new[] { reportType };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
