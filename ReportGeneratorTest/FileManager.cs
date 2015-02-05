using System.IO;
using System.Linq;

namespace Palmmedia.ReportGeneratorTest
{
    internal static class FileManager
    {
        private const string TEMPDIRECTORY = @"C:\temp";

        internal static string GetCSharpReportDirectory()
        {
            return Path.Combine(GetFilesDirectory(), "CSharp", "Reports");
        }

        internal static string GetFSharpReportDirectory()
        {
            return Path.Combine(GetFilesDirectory(), "FSharp", "Reports");
        }

        internal static string GetCSharpCodeDirectory()
        {
            return Path.Combine(GetFilesDirectory(), "CSharp", "Project");
        }

        internal static string GetFSharpCodeDirectory()
        {
            return Path.Combine(GetFilesDirectory(), "FSharp", "Project");
        }

        internal static string GetFilesDirectory()
        {
            var baseDirectory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(baseDirectory, "ReportGenerator.Testprojects");
        }

        internal static void CopyTestClasses()
        {
            if (!Directory.Exists(TEMPDIRECTORY))
            {
                Directory.CreateDirectory(TEMPDIRECTORY);
            }

            var files = new DirectoryInfo(GetCSharpCodeDirectory()).GetFiles("*.cs")
                .Concat(new DirectoryInfo(GetFSharpCodeDirectory()).GetFiles("*.fs"));

            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(TEMPDIRECTORY, fileInfo.Name), true);
            }
        }

        internal static void DeleteTestClasses()
        {
            if (Directory.Exists(TEMPDIRECTORY))
            {
                var files = new DirectoryInfo(TEMPDIRECTORY).GetFiles("*.cs")
                    .Concat(new DirectoryInfo(TEMPDIRECTORY).GetFiles("*.fs"));

                foreach (var fileInfo in files)
                {
                    File.Delete(fileInfo.FullName);
                }

                if (new DirectoryInfo(TEMPDIRECTORY).GetFiles().Length == 0)
                {
                    Directory.Delete(TEMPDIRECTORY);
                }
            }
        }
    }
}
