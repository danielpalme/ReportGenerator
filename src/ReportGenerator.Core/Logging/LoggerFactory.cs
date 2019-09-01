using System;

namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// Factory for loggers.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// Inner factory.
        /// </summary>
        private static volatile ILoggerFactory innerFactory = new ConsoleLoggerFactory();

        /// <summary>
        /// Gets or sets the verbosity level of loggers.
        /// </summary>
        public static VerbosityLevel VerbosityLevel
        {
            get
            {
                return innerFactory.VerbosityLevel;
            }

            set
            {
                innerFactory.VerbosityLevel = value;
            }
        }

        /// <summary>
        /// Initializes the logger for the given type.
        /// </summary>
        /// <param name="type">The type of the class that uses the logger.</param>
        /// <returns>The logger.</returns>
        public static ILogger GetLogger(Type type) => innerFactory.GetLogger(type);
    }
}
