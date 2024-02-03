namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// Represents the Git information.
    /// </summary>
    internal class GitInformation
    {
        /// <summary>
        /// Gets or sets the branch.
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Gets or sets the sha hash.
        /// </summary>
        public string Sha { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public string TimeStamp { get; set; }
    }
}
