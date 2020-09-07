using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class FileManager
    {
        private const string TEMPDIRECTORY = @"C:\temp";

        private static string filesDirectory;

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
            if (!Directory.Exists(Path.Combine(javaDir, "sub")))
            {
                Directory.CreateDirectory(Path.Combine(javaDir, "sub"));
            }

            var files = new DirectoryInfo(GetCSharpCodeDirectory()).GetFiles("*.cs")
                .Concat(new DirectoryInfo(GetFSharpCodeDirectory()).GetFiles("*.fs"))
                .Concat(new DirectoryInfo(GetCPlusPlusCodeDirectory()).GetFiles("*.cpp"));

            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(TEMPDIRECTORY, fileInfo.Name), true);
            }

            files = new DirectoryInfo(Path.Combine(GetJavaCodeDirectory(), "test")).GetFiles("*.java");

            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(javaDir, fileInfo.Name), true);
            }

            files = new DirectoryInfo(Path.Combine(GetJavaCodeDirectory(), "test", "sub")).GetFiles("*.java");
            foreach (var fileInfo in files)
            {
                File.Copy(fileInfo.FullName, Path.Combine(javaDir, "sub", fileInfo.Name), true);
            }
        }

        internal static string GetCSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Reports");

        internal static string GetFSharpReportDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Reports");

        internal static string GetJavaReportDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Reports");

        internal static string GetCPlusPlusReportDirectory() => Path.Combine(GetFilesDirectory(), "C++", "Reports");

        internal static string GetCSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "CSharp", "Project");

        internal static string GetFSharpCodeDirectory() => Path.Combine(GetFilesDirectory(), "FSharp", "Project");

        internal static string GetJavaCodeDirectory() => Path.Combine(GetFilesDirectory(), "Java", "Project");

        internal static string GetCPlusPlusCodeDirectory() => Path.Combine(GetFilesDirectory(), "C++", "Projects");

        internal static string GetFilesDirectory()
        {
            if (filesDirectory == null)
            {
                var currentDirectory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                while (true)
                {
                    currentDirectory = currentDirectory.Parent;
                    string directory = Path.Combine(currentDirectory.FullName, "Testprojects");

                    if (Directory.Exists(directory))
                    {
                        filesDirectory = directory;
                        break;
                    }
                }
            }

            return filesDirectory;
        }

        internal static string GetTestDirectory() => Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(attr => attr.Key == "ProjectDirectory")
            .Select(attr => attr.Value)
            .FirstOrDefault() ?? Directory.GetCurrentDirectory() + @"..\..";
    }

    [CollectionDefinition("FileManager")]
    public class FileManagerCollection : ICollectionFixture<FileManager>, ICollectionFixture<LimitedVerbosityFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
