using System;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;

namespace Palmmedia.ReportGenerator.Mtp.Logging
{
    internal class ReportGeneratorLoggerAdapterFactory : Palmmedia.ReportGenerator.Core.Logging.ILoggerFactory
    {
        /// <summary>
        /// The cached logger.
        /// </summary>
        private readonly Core.Logging.ILogger logger;

        public ReportGeneratorLoggerAdapterFactory(IExtension extension, ILogger logger, IOutputDevice outputDevice)
        {
            this.logger = new ReportGeneratorLoggerAdapter(extension, logger, outputDevice);
        }

        /// <summary>
        /// Gets or sets the verbosity of delegate loggers.
        /// </summary>
        public Core.Logging.VerbosityLevel VerbosityLevel
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

        public Core.Logging.ILogger GetLogger(Type type)
        {
            return this.logger;
        }
    }
}
