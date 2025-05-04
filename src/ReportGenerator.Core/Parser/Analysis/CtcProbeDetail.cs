using System;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Contains the CTC probe details.
    /// </summary>
    public class CtcProbeDetail
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CtcProbeDetail"/> class.
        /// </summary>
        /// <param name="achived">A value indicating whether the probe was achieved.</param>
        /// <param name="description">the description of the probe.</param>
        public CtcProbeDetail(bool achived, string description)
        {
            this.Achived = achived;
            this.Description = description;
        }

        /// <summary>
        /// Gets a value indicating whether the probe was achieved.
        /// </summary>
        public bool Achived { get; private set; }

        /// <summary>
        /// Gets the description of the probe.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Merges the given CTC probe details with the current instance.
        /// </summary>
        /// <param name="ctcProbeDetail">The CTC probe details to merge.</param>
        internal void Merge(CtcProbeDetail ctcProbeDetail)
        {
            if (ctcProbeDetail == null)
            {
                throw new ArgumentNullException(nameof(ctcProbeDetail));
            }

            if (ctcProbeDetail.Achived)
            {
                this.Achived = true;
            }
        }
    }
}