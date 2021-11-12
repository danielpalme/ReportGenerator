using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class MsBuildTest
    {
        [Fact (Skip = "Not working with Github Action")]
        public void ExecuteMSBuildScript_NetFull()
        {
            string configuration = "Release";

#if DEBUG
            configuration = "Debug";
#endif

            var paths = new[]
            {
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
            };

            bool found = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]))
                {
                    found = true;

                    var log = Path.Combine(
                        Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") ?? FileManager.GetTestDirectory(),
                        nameof(ExecuteMSBuildScript_NetFull) + ".binlog");
                    var processStartInfo = new ProcessStartInfo(
                        paths[i],
                        $"MsBuildTestScript.proj /p:Configuration={configuration} /bl:{log}")
                    {
                        WorkingDirectory = FileManager.GetTestDirectory(),
                        RedirectStandardOutput = true
                    };

                    var process = Process.Start(processStartInfo);
                    Assert.True(process.WaitForExit(5000));

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

            var log = Path.Combine(
                Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") ?? FileManager.GetTestDirectory(),
                nameof(ExecuteMSBuildScript_NetCore) + ".binlog");
            var processStartInfo = new ProcessStartInfo(
                "dotnet",
                $"msbuild MsBuildTestScript_NetCore.proj /p:Configuration={configuration} /bl:{log}")
            {
                WorkingDirectory = FileManager.GetTestDirectory(),
                RedirectStandardOutput = true
            };

            var process = Process.Start(processStartInfo);
            Assert.True(process.WaitForExit(5000));

            Assert.True(0 == process.ExitCode, process.StandardOutput.ReadToEnd());
        }
    }
}
