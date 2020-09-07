using System;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Exception indicating that a parser/xml format is not or no longer supported.
    /// </summary>
    [Serializable]
    internal class UnsupportedParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedParserException"/> class.
        /// </summary>
        public UnsupportedParserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public UnsupportedParserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public UnsupportedParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
