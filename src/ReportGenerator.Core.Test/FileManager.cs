using System.IO;
using System.Linq;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class FileManager
    {
        private const string TEMPDIRECTORY = @"C:\temp";

        public FileManager()
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

        internal static string GetCSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Reports");

        internal static string GetFSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Reports");

        internal static string GetJavaReportDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Reports");

        internal static string GetCSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Project");

        internal static string GetFSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Project");

        internal static string GetJavaCodeDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Project");

        internal static string GetFilesDirectory()
        {
            var baseDirectory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(baseDirectory, "Testprojects");
        }
    }

    [CollectionDefinition("FileManager")]
    public class FileManagerCollection : ICollectionFixture<FileManager>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
