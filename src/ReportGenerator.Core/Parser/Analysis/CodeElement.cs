using System;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents an element (e.g. a method or property) in a code file.
    /// </summary>
    public class CodeElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeElement" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The <see cref="Analysis.CodeElementType"/>.</param>
        /// <param name="firstLine">The number of the first line.</param>
        /// <param name="lastLine">The number of the last line.</param>
        /// <param name="coverageQuota">The coverage quota.</param>
        internal CodeElement(string name, CodeElementType type, int firstLine, int lastLine, decimal? coverageQuota)
            : this(name, name, type, firstLine, lastLine, coverageQuota)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeElement" /> class.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The <see cref="Analysis.CodeElementType"/>.</param>
        /// <param name="firstLine">The number of the first line.</param>
        /// <param name="lastLine">The number of the last line.</param>
        /// <param name="coverageQuota">The coverage quota.</param>
        internal CodeElement(string fullName, string name, CodeElementType type, int firstLine, int lastLine, decimal? coverageQuota)
        {
            this.FullName = fullName ?? throw new ArgumentNullException(nameof(name));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.CodeElementType = type;
            this.FirstLine = firstLine;
            this.LastLine = lastLine;
            if (coverageQuota.HasValue)
            {
                this.CoverageQuota = Math.Min(100, Math.Max(0, coverageQuota.Value));
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="Analysis.CodeElementType"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Analysis.CodeElementType"/>.
        /// </value>
        public CodeElementType CodeElementType { get; }

        /// <summary>
        /// Gets the number of the first line.
        /// </summary>
        /// <value>
        /// The number of the first line.
        /// </value>
        public int FirstLine { get; }

        /// <summary>
        /// Gets the number of the last line.
        /// </summary>
        /// <value>
        /// The number of the last line.
        /// </value>
        public int LastLine { get; }

        /// <summary>
        /// Gets the coverage quota of the code element.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota { get; private set; }

        /// <summary>
        /// Applies the given coverage quota if greater than existing quota.
        /// </summary>
        /// <param name="quota">The quota.</param>
        public void ApplyMaximumCoverageQuota(decimal? quota)
        {
            if (quota.HasValue)
            {
                if (this.CoverageQuota.HasValue)
                {
                    this.CoverageQuota = Math.Max(quota.Value, this.CoverageQuota.Value);
                }
                else
                {
                    this.CoverageQuota = quota;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(CodeElement)))
            {
                return false;
            }
            else
            {
                var codeElement = (CodeElement)obj;
                return this.FullName.Equals(codeElement.FullName) && this.FirstLine == codeElement.FirstLine;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.FullName.GetHashCode() + this.FirstLine.GetHashCode();
    }
}
