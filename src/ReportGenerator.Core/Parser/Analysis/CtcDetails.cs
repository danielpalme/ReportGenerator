using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Contains the CTC details.
    /// </summary>
    public class CtcDetails
    {
        /// <summary>
        /// The true/false combinations.
        /// </summary>
        private readonly List<CtcProbeDetail> trueFalseCombinations;

        /// <summary>
        /// The MC/DC information.
        /// </summary>
        private readonly List<CtcProbeDetail> mcdcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtcDetails"/> class.
        /// </summary>
        /// <param name="trueFalseCombinations">The true/false combinations.</param>
        /// <param name="mcdcs">The MC/DC information.</param>
        public CtcDetails(
            List<CtcProbeDetail> trueFalseCombinations,
            List<CtcProbeDetail> mcdcs)
        {
            this.trueFalseCombinations = trueFalseCombinations ?? throw new ArgumentNullException(nameof(trueFalseCombinations));
            this.mcdcs = mcdcs ?? throw new ArgumentNullException(nameof(mcdcs));
        }

        /// <summary>
        /// Gets the true/false combinations.
        /// </summary>
        public IReadOnlyCollection<CtcProbeDetail> TrueFalseCombinations => this.trueFalseCombinations;

        /// <summary>
        /// Gets the MC/DC information.
        /// </summary>
        public IReadOnlyCollection<CtcProbeDetail> Mcdcs => this.mcdcs;

        /// <summary>
        /// Merges the given CTC details with the current instance.
        /// </summary>
        /// <param name="ctcDetails">The CTC details to merge.</param>
        internal void Merge(CtcDetails ctcDetails)
        {
            if (ctcDetails == null)
            {
                throw new ArgumentNullException(nameof(ctcDetails));
            }

            foreach (CtcProbeDetail ctcProbeDetail in ctcDetails.TrueFalseCombinations)
            {
                var existingProbeDetail = this.trueFalseCombinations.FirstOrDefault(x => x.Description == ctcProbeDetail.Description);
                if (existingProbeDetail != null)
                {
                    existingProbeDetail.Merge(ctcProbeDetail);
                }
                else
                {
                    this.trueFalseCombinations.Add(ctcProbeDetail);
                }
            }

            foreach (CtcProbeDetail ctcProbeDetail in ctcDetails.Mcdcs)
            {
                var existingProbeDetail = this.mcdcs.FirstOrDefault(x => x.Description == ctcProbeDetail.Description);
                if (existingProbeDetail != null)
                {
                    existingProbeDetail.Merge(ctcProbeDetail);
                }
                else
                {
                    this.mcdcs.Add(ctcProbeDetail);
                }
            }
        }
    }
}