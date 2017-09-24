using System;

namespace Palmmedia.ReportGenerator.Logging
{
    /// <summary>
    /// Factory for loggers.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// Inner factory synchronization object.
        /// </summary>
        private static readonly object InnerFactorySync = new object();

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

        /// <summary>
        /// Configures the inner logger factory.
        /// </summary>
        /// <param name="factory">The inner logger factory.</param>
        public static void Configure(ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            lock (InnerFactorySync)
            {
                innerFactory = factory;
            }
        }
    }
}
