using System.Diagnostics;
using System.IO;
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

            var paths = new[]
            {
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
            };

            bool found = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]))
                {
                    found = true;

                    var processStartInfo = new ProcessStartInfo(
                        paths[i],
                        $"MsBuildTestScript.proj /p:Configuration={configuration}")
                    {
                        WorkingDirectory = FileManager.GetTestDirectory(),
                        RedirectStandardOutput = true
                    };

                    var process = Process.Start(processStartInfo);
                    process.WaitForExit();

                    Assert.True(0 == process.ExitCode, process.StandardOutput.ReadToEnd());

                    break;
                }
            }

            Assert.True(found, "MsBuild was not found");
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
