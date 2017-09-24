using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palmmedia.ReportGenerator.Logging
{
    /// <summary>
    /// A logger factory creating console loggers.
    /// </summary>
    internal class ConsoleLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// The cached logger.
        /// </summary>
        private static readonly ILogger Logger = new ConsoleLogger();

        /// <summary>
        /// Gets or sets the verbosity of console loggers.
        /// </summary>
        public VerbosityLevel VerbosityLevel
        {
            get
            {
                return Logger.VerbosityLevel;
            }

            set
            {
                Logger.VerbosityLevel = value;
            }
        }

        /// <summary>
        /// Initializes the logger for the given type.
        /// </summary>
        /// <param name="type">The type of the class that uses the logger.</param>
        /// <returns>The logger.</returns>
        public ILogger GetLogger(Type type) => Logger;
    }
}
