using System;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Exception indicating that mimimum coverage goals are not satisfied.
    /// </summary>
    [Serializable]
    internal class LowCoverageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LowCoverageException"/> class.
        /// </summary>
        public LowCoverageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowCoverageException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public LowCoverageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowCoverageException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public LowCoverageException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}