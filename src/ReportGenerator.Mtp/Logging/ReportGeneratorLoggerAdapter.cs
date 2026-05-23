using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;

namespace Palmmedia.ReportGenerator.Mtp.Logging
{
    internal class ReportGeneratorLoggerAdapter : Core.Logging.ILogger, IOutputDeviceDataProducer
    {
        private readonly IExtension extension;

        private readonly ILogger logger;

        private readonly IOutputDevice outputDevice;

        public ReportGeneratorLoggerAdapter(
            IExtension extension,
            ILogger logger,
            IOutputDevice outputDevice)
        {
            this.extension = extension;
            this.logger = logger;
            this.outputDevice = outputDevice;
        }

        public string Uid => this.extension.Uid;

        public string Version => this.extension.Version;

        public string DisplayName => this.extension.DisplayName;

        public string Description => this.extension.Description;

        public Task<bool> IsEnabledAsync() => Task.FromResult(true);

        /// <summary>
        /// Gets or sets the verbosity level.
        /// </summary>
        public Core.Logging.VerbosityLevel VerbosityLevel { get; set; }

        /// <summary>
        /// Log a message at DEBUG level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Info)
            {
                this.logger.LogDebug(message);

                this.outputDevice.DisplayAsync(
                    this,
                    new TextOutputDeviceData($"[ReportGenerator] " + message),
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a formatted message at DEBUG level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Info)
            {
                string message = string.Format(format, args);

                this.logger.LogDebug(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new TextOutputDeviceData($"[ReportGenerator] " + message),
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Warning)
            {
                this.logger.LogInformation(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new TextOutputDeviceData($"[ReportGenerator] " + message),
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a formatted message at INFO level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Warning)
            {
                string message = string.Format(format, args);

                this.logger.LogInformation(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new TextOutputDeviceData($"[ReportGenerator] " + message),
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a message at WARN level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn(string message)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Error)
            {
                this.logger.LogWarning(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new FormattedTextOutputDeviceData($"[ReportGenerator] " + message)
                    {
                        ForegroundColor = new SystemConsoleColor { ConsoleColor = ConsoleColor.Magenta }
                    },
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a formatted message at WARN level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Error)
            {
                string message = string.Format(format, args);

                this.logger.LogWarning(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new FormattedTextOutputDeviceData($"[ReportGenerator] " + message)
                    {
                        ForegroundColor = new SystemConsoleColor { ConsoleColor = ConsoleColor.Magenta }
                    },
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a message at INFO level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Off)
            {
                this.logger.LogError(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new FormattedTextOutputDeviceData($"[ReportGenerator] " + message)
                    {
                        ForegroundColor = new SystemConsoleColor { ConsoleColor = ConsoleColor.Red }
                    },
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Log a formatted message at ERROR level.
        /// </summary>
        /// <param name="format">The template string.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (this.VerbosityLevel < Core.Logging.VerbosityLevel.Off)
            {
                string message = string.Format(format, args);

                this.logger.LogError(message);
                this.outputDevice.DisplayAsync(
                    this,
                    new FormattedTextOutputDeviceData($"[ReportGenerator] " + message)
                    {
                        ForegroundColor = new SystemConsoleColor { ConsoleColor = ConsoleColor.Red }
                    },
                    CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
