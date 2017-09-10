using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Palmmedia.ReportGeneratorTest
{
    [TestClass]
    public static class FileManager
    {
        private const string TEMPDIRECTORY = @"C:\temp";

        [AssemblyInitialize]
        public static void CopyTestClasses(TestContext context)
        {
            if (!Directory.Exists(TEMPDIRECTORY))
            {
                Directory.CreateDirectory(TEMPDIRECTORY);
            }

            string javaDir = Path.Combine(TEMPDIRECTORY, "test");
            if (!Directory.Exists(javaDir))
            {
                Directory.CreateDirectory(javaDir);
            }

            var files = new DirectoryInfo(GetCSharpCodeDirectory()).GetFiles("*.cs")
                .Concat(new DirectoryInfo(GetFSharpCodeDirectory()).GetFiles("*.fs"));

            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(TEMPDIRECTORY, fileInfo.Name), true);
            }

            files = new DirectoryInfo(Path.Combine(GetJavaCodeDirectory(), "test")).GetFiles("*.java");

            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(javaDir, fileInfo.Name), true);
            }
        }

        [AssemblyCleanup]
        public static void DeleteTestClasses()
        {
            if (Directory.Exists(TEMPDIRECTORY))
            {
                var files = new DirectoryInfo(TEMPDIRECTORY).GetFiles("*.cs")
                    .Concat(new DirectoryInfo(TEMPDIRECTORY).GetFiles("*.fs"))
                    .Concat(new DirectoryInfo(TEMPDIRECTORY).GetFiles("*.java"));

                foreach (var fileInfo in files)
                {
                    File.Delete(fileInfo.FullName);
                }

                string javaDir = Path.Combine(TEMPDIRECTORY, "test");
                files = new DirectoryInfo(javaDir).GetFiles("*.java");

                foreach (var fileInfo in files)
                {
                    File.Delete(fileInfo.FullName);
                }

                Directory.Delete(javaDir);

                if (!Directory.EnumerateFiles(TEMPDIRECTORY).Any())
                {
                    Directory.Delete(TEMPDIRECTORY);
                }
            }
        }

        internal static string GetCSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Reports");

        internal static string GetFSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Reports");

        internal static string GetJavaReportDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Reports");

        internal static string GetCSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Project");

        internal static string GetFSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Project");

        internal static string GetJavaCodeDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Project");

        internal static string GetFilesDirectory()
        {
            var baseDirectory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(baseDirectory, "ReportGenerator.Testprojects");
        }
    }
}
