using System.Diagnostics;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class MsBuildTest
    {

        [Fact]
        public void ExecuteMSBuildScript_NetFull()
        {
            string configuration = "Release";

#if DEBUG
            configuration = "Debug";
#endif

            var processStartInfo = new ProcessStartInfo(
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
                $"MsBuildTestScript.proj /p:Configuration={configuration}")
            {
                WorkingDirectory = FileManager.GetTestDirectory(),
                RedirectStandardOutput = true
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            Assert.True(0 == process.ExitCode, process.StandardOutput.ReadToEnd());
        }

        [Fact]
        public void ExecuteMSBuildScript_NetCore()
        {
            string configuration = "Release";

#if DEBUG
            configuration = "Debug";
#endif

            var processStartInfo = new ProcessStartInfo(
                "dotnet",
                $"msbuild MsBuildTestScript_NetCore.proj /p:Configuration={configuration}")
            {
                WorkingDirectory = FileManager.GetTestDirectory(),
                RedirectStandardOutput = true
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            Assert.True(0 == process.ExitCode, process.StandardOutput.ReadToEnd());
        }
    }
}
