using System;

namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// A logger factory creating delegate loggers.
    /// </summary>
    internal class DelegateLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// The cached logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateLoggerFactory"/> class.
        /// </summary>
        /// <param name="logDelegate">The log delegate.</param>
        public DelegateLoggerFactory(Action<VerbosityLevel, string> logDelegate)
        {
            if (logDelegate == null)
            {
                throw new ArgumentNullException(nameof(logDelegate));
            }

            this.logger = new DelegateLogger(logDelegate);
        }

        /// <summary>
        /// Gets or sets the verbosity of delegate loggers.
        /// </summary>
        public VerbosityLevel VerbosityLevel
        {
            get
            {
                return this.logger.VerbosityLevel;
            }

            set
            {
                this.logger.VerbosityLevel = value;
            }
        }

        /// <summary>
        /// Initializes the logger for the given type.
        /// </summary>
        /// <param name="type">The type of the class that uses the logger.</param>
        /// <returns>The logger.</returns>
        public ILogger GetLogger(Type type) => this.logger;
    }
}
