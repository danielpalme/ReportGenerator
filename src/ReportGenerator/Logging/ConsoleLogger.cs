using System;

namespace Palmmedia.ReportGenerator.Logging
{
    /// <summary>
    /// <see cref="ILogger"/> that writes to the console.
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        public VerbosityLevel VerbosityLevel { get; set; }

        /// <summary>
        /// Log a message at DEBUG level.
        /// </summary>
        /// <param name="message">The message</param>
        public void Debug(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Info)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Log a formatted message at DEBUG level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Info)
            {
                Console.WriteLine(format, args);
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message</param>
        public void Info(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Warning)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Log a formatted message at INFO level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Warning)
            {
                Console.WriteLine(format, args);
            }
        }

        /// <summary>
        /// Log a message at WARN level.
        /// </summary>
        /// <param name="message">The message</param>
        public void Warn(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log a formatted message at WARN level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message</param>
        public void Error(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Off)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log a formatted message at ERROR level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < VerbosityLevel.Off)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }
        }
    }
}
