namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// Interface for loggers.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        VerbosityLevel VerbosityLevel { get; set; }

        /// <summary>
        /// Log a message at DEBUG level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(string message);

        /// <summary>
        /// Log a formatted message at DEBUG level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        void DebugFormat(string format, params object[] args);

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>
        /// Log a formatted message at INFO level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        /// Log a message at WARN level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warn(string message);

        /// <summary>
        /// Log a formatted message at WARN level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        void WarnFormat(string format, params object[] args);

        /// <summary>
        /// Log a message at ERROR level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);

        /// <summary>
        /// Log a formatted message at ERROR level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        void ErrorFormat(string format, params object[] args);
    }
}
