using System;

namespace Palmmedia.ReportGenerator.Core.Logging
{
    /// <summary>
    /// Provides <see cref="ILogger"/> objects creation capabilities.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Gets or sets the verbosity of <see cref="ILogger"/> objects.
        /// </summary>
        VerbosityLevel VerbosityLevel { get; set; }

        /// <summary>
        /// Initializes the logger for the given type.
        /// </summary>
        /// <param name="type">The type of the class that uses the logger.</param>
        /// <returns>The logger.</returns>
        ILogger GetLogger(Type type);
    }
}
