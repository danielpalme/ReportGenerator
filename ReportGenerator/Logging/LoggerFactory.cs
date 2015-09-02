using System;

namespace Palmmedia.ReportGenerator.Logging
{
    /// <summary>
    /// Factory for loggers.
    /// </summary>
    internal class LoggerFactory
    {
        /// <summary>
        /// The cached logger.
        /// </summary>
        private static readonly ILogger Logger = new ConsoleLogger();

        /// <summary>
        /// Sets the verbosity level.
        /// </summary>
        public static VerbosityLevel VerbosityLevel
        {
            set
            {
                Logger.VerbosityLevel = value;
            }
        }

        /// <summary>
        /// Initializes the logger for the given type.
        /// </summary>
        /// <param name="tpye">The type of the class that uses the logger.</param>
        /// <returns>The logger.</returns>
        public static ILogger GetLogger(Type tpye) => Logger;
    }
}
