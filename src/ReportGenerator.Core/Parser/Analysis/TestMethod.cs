using System;
using System.Threading;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Name of a test method.
    /// </summary>
    public class TestMethod
    {
        /// <summary>
        /// Counter for unique ids.
        /// </summary>
        private static long counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethod" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shortName">The short name.</param>
        internal TestMethod(string name, string shortName)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
            this.Id = Interlocked.Increment(ref counter);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName { get; }

        /// <summary>
        /// Gets the id of the test method.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public long Id { get; }

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
            if (obj == null || !obj.GetType().Equals(typeof(TestMethod)))
            {
                return false;
            }
            else
            {
                var testMethod = (TestMethod)obj;
                return testMethod.Name.Equals(this.Name);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.Name.GetHashCode();
    }
}
