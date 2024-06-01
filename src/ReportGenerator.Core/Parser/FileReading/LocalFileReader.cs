using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.FileReading
{
    /// <summary>
    /// File reader for reading files from local disk.
    /// </summary>
    internal class LocalFileReader : IFileReader
    {
        /// <summary>
        /// Regex to analyze if a path is a deterministic path.
        /// </summary>
        private static readonly Regex DeterministicPathRegex = new Regex("\\/_\\d?\\/", RegexOptions.Compiled);

        /// <summary>
        /// The source directories for typical environments like Azure DevOps or Github Actions.
        /// </summary>
        private static readonly IReadOnlyList<string> DeterministicSourceDirectories;

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IReadOnlyList<string> sourceDirectories;

        static LocalFileReader()
        {
            var directories = new List<string>();

            // Azure DevOps
            if ("true".Equals(Environment.GetEnvironmentVariable("TF_BUILD"), StringComparison.OrdinalIgnoreCase)
                && Environment.GetEnvironmentVariable("Build.SourcesDirectory") != null)
            {
                directories.Add(Environment.GetEnvironmentVariable("Build.SourcesDirectory"));
            }

            // Github Actions
            if ("true".Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), StringComparison.OrdinalIgnoreCase)
                && Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") != null)
            {
                directories.Add(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE"));
            }

            DeterministicSourceDirectories = directories;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileReader" /> class.
        /// </summary>
        public LocalFileReader()
            : this(Enumerable.Empty<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileReader" /> class.
        /// </summary>
        /// <param name="sourceDirectories">The source directories.</param>
        public LocalFileReader(IEnumerable<string> sourceDirectories)
        {
            if (sourceDirectories == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectories));
            }

            this.sourceDirectories = sourceDirectories.ToList();
        }

        /// <summary>
        /// Loads the file with the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="error">Error message if file reading failed, otherwise <code>null</code>.</param>
        /// <returns><code>null</code> if an error occurs, otherwise the lines of the file.</returns>
        public string[] LoadFile(string path, out string error)
        {
            string mappedPath = this.MapPath(path);

            try
            {
                if (!File.Exists(mappedPath))
                {
                    error = string.Format(CultureInfo.InvariantCulture, Resources.FileDoesNotExist, path);
                    return null;
                }

                var encoding = FileHelper.GetEncoding(mappedPath);
                string[] lines = File.ReadAllLines(mappedPath, encoding);

                error = null;
                return lines;
            }
            catch (Exception ex)
            {
                error = string.Format(CultureInfo.InvariantCulture, Resources.ErrorDuringReadingFile, path, ex.GetExceptionMessageForDisplay());
                return null;
            }
        }

        private string MapPath(string path)
        {
            if (File.Exists(path))
            {
                return path;
            }

            if (path.StartsWith("/_") && DeterministicPathRegex.IsMatch(path))
            {
                path = path.Substring(path.IndexOf("/", 2) + 1);

                if (File.Exists(path))
                {
                    return path;
                }

                if (this.sourceDirectories.Count == 0)
                {
                    return MapPath(path, DeterministicSourceDirectories);
                }
            }

            if (this.sourceDirectories.Count > 0)
            {
                return MapPath(path, this.sourceDirectories);
            }

            return path;
        }

        private static string MapPath(string path, IEnumerable<string> directories)
        {
            /*
             * Search in source dirctories
             *
             * E.g. with source directory 'C:\agent\1\work\s' the following locations will be searched:
             * C:\agent\1\work\s\_\some\directory\file.cs
             * C:\agent\1\work\s\some\directory\file.cs
             * C:\agent\1\work\s\directory\file.cs
             * C:\agent\1\work\s\file.cs
             */
            string[] parts = path.Split('/', '\\');

            foreach (var sourceDirectory in directories)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    string combinedPath = sourceDirectory;

                    for (int j = i; j < parts.Length; j++)
                    {
                        combinedPath = Path.Combine(combinedPath, parts[j]);
                    }

                    if (File.Exists(combinedPath))
                    {
                        return combinedPath;
                    }
                }
            }

            return path;
        }
    }
}
