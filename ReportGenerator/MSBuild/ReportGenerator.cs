using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGenerator.MSBuild
{
    /// <summary>
    /// MSBuild Task for generating reports.
    /// </summary>
    /// <example>
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;<br/>
    /// &lt;Project DefaultTargets="Coverage" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0"&gt;<br/>
    ///   &lt;UsingTask TaskName="ReportGenerator" AssemblyFile="ReportGenerator.exe" /&gt;<br/>
    ///   &lt;ItemGroup&gt;<br/>
    ///       &lt;CoverageFiles Include="partcover.xml" /&gt;<br/>
    ///   &lt;/ItemGroup&gt;<br/>
    ///   &lt;Target Name="Coverage"&gt;<br/>
    ///     &lt;ReportGenerator ReportFiles="@(CoverageFiles)" TargetDirectory="report" ReportTypes="Html" /&gt;<br/>
    ///   &lt;/Target&gt;<br/>
    /// &lt;/Project&gt;
    /// </example>
    public class ReportGenerator : Task, ITask
    {
        /// <summary>
        /// Gets or sets the report files.
        /// </summary>
        [Required]
        public ITaskItem[] ReportFiles { get; set; }

        /// <summary>
        /// Gets or sets the directory the report will be created in. This must be a directory, not a file. If the directory does not exist, it is created automatically. 
        /// </summary>
        [Required]
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory the historic data will be created in. This must be a directory, not a file. If the directory does not exist, it is created automatically. 
        /// </summary>
        public string HistoryDirectory { get; set; }

        /// <summary>
        /// Gets or sets the types of the report.
        /// </summary>
        /// <value>The types of the report.</value>
        public ITaskItem[] ReportTypes { get; set; }

        /// <summary>
        /// Gets or sets the source directories. Optional directories which contain the corresponding source code. The source files are used if coverage report contains classes without path information.
        /// </summary>
        /// <value>
        /// The source directories.
        /// </value>
        public ITaskItem[] SourceDirectories { get; set; }

        /// <summary>
        /// Gets or sets the assembly filters (old property).
        /// </summary>
        /// <value>
        /// The assembly filters.
        /// </value>
        public ITaskItem[] Filters { get; set; }

        /// <summary>
        /// Gets or sets the assembly filters (new property).
        /// </summary>
        /// <value>
        /// The assembly filters.
        /// </value>
        public ITaskItem[] AssemblyFilters { get; set; }

        /// <summary>
        /// Gets or sets the class filters.
        /// </summary>
        /// <value>
        /// The class filters.
        /// </value>
        public ITaskItem[] ClassFilters { get; set; }

        /// <summary>
        /// Gets or sets the file filters.
        /// </summary>
        /// <value>
        /// The file filters.
        /// </value>
        public ITaskItem[] FileFilters { get; set; }

        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        /// <value>
        /// The verbosity level.
        /// </value>
        public string VerbosityLevel { get; set; }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            string[] reportTypes = new string[] { };

            if (this.ReportTypes != null && this.ReportTypes.Length > 0)
            {
                reportTypes = this.ReportTypes.Select(r => r.ItemSpec).ToArray();
            }

            ITaskItem[] assemblyFilters = this.AssemblyFilters ?? this.Filters;

            ReportConfiguration configuration = new ReportConfiguration(
                new MefReportBuilderFactory(),
                this.ReportFiles == null ? Enumerable.Empty<string>() : this.ReportFiles.Select(r => r.ItemSpec),
                this.TargetDirectory,
                this.HistoryDirectory,
                reportTypes,
                this.SourceDirectories == null ? Enumerable.Empty<string>() : this.SourceDirectories.Select(r => r.ItemSpec),
                assemblyFilters == null ? Enumerable.Empty<string>() : assemblyFilters.Select(r => r.ItemSpec),
                this.ClassFilters == null ? Enumerable.Empty<string>() : this.ClassFilters.Select(r => r.ItemSpec),
                this.FileFilters == null ? Enumerable.Empty<string>() : this.FileFilters.Select(r => r.ItemSpec),
                this.VerbosityLevel);

            return new Generator().GenerateReport(configuration);
        }
    }
}
