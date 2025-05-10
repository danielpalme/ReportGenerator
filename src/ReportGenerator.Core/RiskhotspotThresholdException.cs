using System;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Exception indicating that mimimum coverage goals are not satisfied.
    /// </summary>
    [Serializable]
    internal class RiskhotspotThresholdException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RiskhotspotThresholdException"/> class.
        /// </summary>
        public RiskhotspotThresholdException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskhotspotThresholdException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public RiskhotspotThresholdException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskhotspotThresholdException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public RiskhotspotThresholdException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}