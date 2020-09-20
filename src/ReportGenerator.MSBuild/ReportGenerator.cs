using System;
using System.IO;
using System.Linq;
using DotNetConfig;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Palmmedia.ReportGenerator.Core;

namespace Palmmedia.ReportGenerator.MSBuild
{
    /// <summary>
    /// MSBuild Task for generating reports.
    /// </summary>
    /// <example>
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;<br/>
    /// &lt;Project DefaultTargets="Coverage" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0"&gt;<br/>
    ///   &lt;ItemGroup&gt;<br/>
    ///       &lt;PackageReference Include="ReportGenerator" Version="x.y.z" /&gt;<br/>
    ///   &lt;/ItemGroup&gt;<br/>
    ///   &lt;ItemGroup&gt;<br/>
    ///       &lt;CoverageFiles Include="OpenCover.xml" /&gt;<br/>
    ///   &lt;/ItemGroup&gt;<br/>
    ///   &lt;Target Name="Coverage"&gt;<br/>
    ///     &lt;ReportGenerator ReportFiles="@(CoverageFiles)" TargetDirectory="report" ReportTypes="Html;Latex" HistoryDirectory="history" Plugins="CustomReports.dll" AssemblyFilters="+Include;-Excluded" VerbosityLevel="Verbose" /&gt;<br/>
    ///   &lt;/Target&gt;<br/>
    /// &lt;/Project&gt;
    /// </example>
    public class ReportGenerator : Task, ITask
    {
        /// <summary>
        /// Gets or sets the project directory where the tool is being run, for loading 
        /// the relevant .netconfig.
        /// </summary>
        public string ProjectDirectory { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets the report files.
        /// </summary>
        public ITaskItem[] ReportFiles { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the directory the report will be created in. This must be a directory, not a file. If the directory does not exist, it is created automatically.
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory the historic data will be created in. This must be a directory, not a file. If the directory does not exist, it is created automatically.
        /// </summary>
        public string HistoryDirectory { get; set; }

        /// <summary>
        /// Gets or sets the types of the report.
        /// </summary>
        /// <value>The types of the report.</value>
        public ITaskItem[] ReportTypes { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the source directories. Optional directories which contain the corresponding source code. The source files are used if coverage report contains classes without path information.
        /// </summary>
        /// <value>
        /// The source directories.
        /// </value>
        public ITaskItem[] SourceDirectories { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>
        /// The plugins.
        /// </value>
        public ITaskItem[] Plugins { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the assembly filters (old property).
        /// </summary>
        /// <value>
        /// The assembly filters.
        /// </value>
        public ITaskItem[] Filters { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the assembly filters (new property).
        /// </summary>
        /// <value>
        /// The assembly filters.
        /// </value>
        public ITaskItem[] AssemblyFilters { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the class filters.
        /// </summary>
        /// <value>
        /// The class filters.
        /// </value>
        public ITaskItem[] ClassFilters { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the file filters.
        /// </summary>
        /// <value>
        /// The file filters.
        /// </value>
        public ITaskItem[] FileFilters { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        /// <value>
        /// The verbosity level.
        /// </value>
        public string VerbosityLevel { get; set; }

        /// <summary>
        /// Gets or sets the custom tag (e.g. build number).
        /// </summary>
        /// <value>
        /// The custom tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            var reportFilePatterns = Array.Empty<string>();
            var targetDirectory = TargetDirectory;
            var sourceDirectories = Array.Empty<string>();
            string historyDirectory = HistoryDirectory;
            var reportTypes = Array.Empty<string>();
            var plugins = Array.Empty<string>();
            var assemblyFilters = Array.Empty<string>();
            var classFilters = Array.Empty<string>();
            var fileFilters = Array.Empty<string>();
            string verbosityLevel = VerbosityLevel;
            string tag = Tag;

            var config = Config.Build(ProjectDirectory).GetSection(DotNetConfigSettingNames.SectionName);
            string value = null;

            if (ReportFiles.Length > 0)
            {
                reportFilePatterns = ReportFiles.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Reports, out value))
            {
                reportFilePatterns = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                reportFilePatterns = config
                    .GetAll(DotNetConfigSettingNames.Report)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (reportFilePatterns.Length == 0)
            {
                Log.LogError($"{nameof(ReportFiles)} is required.");
                return false;
            }

            if (string.IsNullOrEmpty(targetDirectory) &&
                config.TryGetString(DotNetConfigSettingNames.TargetDirectory, out value))
            {
                targetDirectory = value;
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                Log.LogError($"{nameof(TargetDirectory)} is required.");
                return false;
            }

            if (SourceDirectories.Length > 0)
            {
                sourceDirectories = SourceDirectories.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.SourceDirectories, out value))
            {
                sourceDirectories = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sourceDirectories = config
                    .GetAll(DotNetConfigSettingNames.SourceDirectory)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (string.IsNullOrEmpty(HistoryDirectory) &&
                config.TryGetString(DotNetConfigSettingNames.HistoryDirectory, out value))
            {
                historyDirectory = value;
            }

            if (ReportTypes.Length > 0)
            {
                reportTypes = ReportTypes.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.ReportTypes, out value))
            {
                reportTypes = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                reportTypes = config
                    .GetAll(DotNetConfigSettingNames.ReportType)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (Plugins.Length > 0)
            {
                plugins = Plugins.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Plugins, out value))
            {
                plugins = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                plugins = config
                    .GetAll(DotNetConfigSettingNames.Plugin)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (AssemblyFilters.Length > 0)
            {
                assemblyFilters = AssemblyFilters.Select(r => r.ItemSpec).ToArray();
            }
            else if (Filters.Length > 0)
            {
                assemblyFilters = Filters.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.AssemblyFilters, out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                assemblyFilters = config
                    .GetAll(DotNetConfigSettingNames.AssemblyFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (ClassFilters.Length > 0)
            {
                classFilters = ClassFilters.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.ClassFilters, out value))
            {
                classFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                classFilters = config
                    .GetAll(DotNetConfigSettingNames.ClassFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (FileFilters.Length > 0)
            {
                fileFilters = FileFilters.Select(r => r.ItemSpec).ToArray();
            }
            else if (config.TryGetString(DotNetConfigSettingNames.FileFilters, out value))
            {
                fileFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                fileFilters = config
                    .GetAll(DotNetConfigSettingNames.FileFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (string.IsNullOrEmpty(verbosityLevel) &&
                config.TryGetString(DotNetConfigSettingNames.Verbosity, out value))
            {
                verbosityLevel = value;
            }

            if (string.IsNullOrEmpty(tag) &&
                config.TryGetString(DotNetConfigSettingNames.Tag, out value))
            {
                tag = value;
            }

            var configuration = new ReportConfiguration(
                reportFilePatterns,
                targetDirectory,
                sourceDirectories,
                historyDirectory,
                reportTypes,
                plugins,
                assemblyFilters,
                classFilters,
                fileFilters,
                verbosityLevel,
                tag);

            return new Generator().GenerateReport(configuration);
        }
    }
}
