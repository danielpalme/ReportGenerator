namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// Enumeration for the logging verbosity.
    /// </summary>
    public enum VerbosityLevel
    {
        /// <summary>
        /// All messages are logged.
        /// </summary>
        Verbose,

        /// <summary>
        /// Only important messages are logged.
        /// </summary>
        Info,

        /// <summary>
        /// Only warnings and errors are logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only errors are logged.
        /// </summary>
        Error,

        /// <summary>
        /// Nothing is logged.
        /// </summary>
        Off
    }
}
