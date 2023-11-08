using System;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Indicates the coverage status of a line in a source file.
    /// </summary>
    [Flags]
    internal enum FlagsLineVisitStatus
    {
        /// <summary>
        /// Line can not be covered.
        /// </summary>
        NotCoverable = 0,

        /// <summary>
        /// Line was not covered.
        /// </summary>
        NotCovered = 1,

        /// <summary>
        /// Line was partially covered.
        /// </summary>
        PartiallyCovered = 2,

        /// <summary>
        /// Line was covered.
        /// </summary>
        Covered = 4
    }
}
