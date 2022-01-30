using System;

namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// <see cref="ILogger"/> which executes a delegate.
    /// </summary>
    internal class DelegateLogger : ILogger
    {
        /// <summary>
        /// The log delegate.
        /// </summary>
        private readonly Action<VerbosityLevel, string> logDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateLogger"/> class.
        /// </summary>
        /// <param name="logDelegate">The log delegate.</param>
        public DelegateLogger(Action<VerbosityLevel, string> logDelegate)
        {
            this.logDelegate = logDelegate ?? throw new ArgumentNullException(nameof(logDelegate));
        }

        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        public VerbosityLevel VerbosityLevel { get; set; }

        /// <summary>
        /// Log a message at DEBUG level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Info)
            {
                this.logDelegate(this.VerbosityLevel, message);
            }
        }

        /// <summary>
        /// Log a formatted message at DEBUG level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Info)
            {
                this.logDelegate(this.VerbosityLevel, string.Format(format, args));
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Warning)
            {
                this.logDelegate(this.VerbosityLevel, message);
            }
        }

        /// <summary>
        /// Log a formatted message at INFO level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Warning)
            {
                this.logDelegate(this.VerbosityLevel, string.Format(format, args));
            }
        }

        /// <summary>
        /// Log a message at WARN level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Error)
            {
                this.logDelegate(this.VerbosityLevel, message);
            }
        }

        /// <summary>
        /// Log a formatted message at WARN level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Error)
            {
                this.logDelegate(this.VerbosityLevel, string.Format(format, args));
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Off)
            {
                this.logDelegate(this.VerbosityLevel, message);
            }
        }

        /// <summary>
        /// Log a formatted message at ERROR level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Off)
            {
                this.logDelegate(this.VerbosityLevel, string.Format(format, args));
            }
        }
    }
}
