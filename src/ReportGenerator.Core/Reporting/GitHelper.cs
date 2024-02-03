using System.Diagnostics;

namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// Helper class to retrieve information from Git.
    /// </summary>
    internal static class GitHelper
    {
        /// <summary>
        /// Gets the Git information.
        /// </summary>
        /// <returns>The Git information.</returns>
        public static GitInformation GetGitInformation()
        {
            GitInformation gitInformation = new GitInformation();
            gitInformation.Branch = ExecuteGitCommand("rev-parse --abbrev-ref HEAD");
            gitInformation.Sha = ExecuteGitCommand("rev-parse HEAD");
            gitInformation.TimeStamp = ExecuteGitCommand("show -s --format=%ct");
            return gitInformation;
        }

        /// <summary>
        /// Gets the file hash.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The Git information.</returns>
        public static string GetFileHash(string path)
        {
            return ExecuteGitCommand("hash-object " + path);
        }

        private static string ExecuteGitCommand(string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return output.Trim();
                }
            }
            catch (System.Exception)
            {
                return string.Empty;
            }
        }
    }
}
