using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Palmmedia.ReportGenerator.Core.Logging
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
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            if (this.VerbosityLevel < VerbosityLevel.Info)
            {
                this.WriteLine(message, ConsoleColor.White);
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
                this.WriteLine(ConsoleColor.White, format, args);
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
                this.WriteLine(message, ConsoleColor.White);
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
                this.WriteLine(ConsoleColor.White, format, args);
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
                this.WriteLine(message, ConsoleColor.Magenta);
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
                this.WriteLine(ConsoleColor.Magenta, format, args);
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
                this.WriteLine(message, ConsoleColor.Red);
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
                this.WriteLine(ConsoleColor.Red, format, args);
            }
        }

        /// <summary>
        /// Write a message to the console.
        /// </summary>
        /// <param name="consoleColor">The color of the console to write the text with.</param>
        /// <param name="format">The template string.</param>
        /// <param name="args">The argument for the template string.</param>
        private void WriteLine(ConsoleColor consoleColor, string format, params object[] args)
        {
            NonBlockingConsole.WriteLine(string.Format($"{DateTime.Now:s}: {format}", args), consoleColor);
        }

        /// <summary>
        /// Write a message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="consoleColor">The color of the console to write the text with.</param>
        private void WriteLine(string message, ConsoleColor consoleColor)
        {
            NonBlockingConsole.WriteLine($"{DateTime.Now:s}: {message}", consoleColor);
        }

        /// <summary>
        /// The non blocking console class.
        /// </summary>
        private static class NonBlockingConsole
        {
            /// <summary>
            /// Queue containing the log messages.
            /// </summary>
            private static readonly BlockingCollection<Tuple<string, ConsoleColor>> Queue = new BlockingCollection<Tuple<string, ConsoleColor>>();

            /// <summary>
            /// Initializes static members of the <see cref="NonBlockingConsole"/> class.
            /// </summary>
            static NonBlockingConsole()
            {
                var thread = new Thread(
                    () =>
                    {
                        while (true)
                        {
                            var item = Queue.Take();
                            Console.ForegroundColor = item.Item2;
                            Console.WriteLine(item.Item1);
                            Console.ResetColor();
                        }
                    });
                thread.IsBackground = true;
                thread.Start();
            }

            /// <summary>
            /// Write the message to the console in a non blocking manner.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="color">The color of the console to write the text with.</param>
            public static void WriteLine(string message, ConsoleColor color)
            {
                Queue.Add(new Tuple<string, ConsoleColor>(message, color));
            }
        }
    }
}
