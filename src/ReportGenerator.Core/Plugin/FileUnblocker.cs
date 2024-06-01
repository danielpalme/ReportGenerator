using System;
using System.Runtime.InteropServices;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// Helper class for unblocking files.
    /// </summary>
    internal static class FileUnblocker
    {
        /// <summary>
        /// Unblocks the given file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if file was unblocked successfully; otherwise, <c>false</c>.</returns>
        public static bool Unblock(string fileName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return DeleteFile(fileName + ":Zone.Identifier");
            }

            return true;
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if file was deleted successfully; otherwise, <c>false</c>.</returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);
    }
}
