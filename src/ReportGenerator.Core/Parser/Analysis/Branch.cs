namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents a branch.
    /// </summary>
    public class Branch
    {
        /// <summary>
        /// The unique identifier of the branch.
        /// </summary>
        private readonly string identifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Branch" /> class.
        /// </summary>
        /// <param name="branchVisits">The number of branch visits.</param>
        /// <param name="identifier">The identifier.</param>
        internal Branch(int branchVisits, string identifier)
        {
            this.BranchVisits = branchVisits;
            this.identifier = identifier;
        }

        /// <summary>
        /// Gets the number of branch visits.
        /// </summary>
        public int BranchVisits { get; internal set; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.identifier;
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
            if (obj == null || !obj.GetType().Equals(typeof(Branch)))
            {
                return false;
            }
            else
            {
                var branch = (Branch)obj;
                return branch.identifier.Equals(this.identifier);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.identifier.GetHashCode();
    }
}
