using System;

namespace Palmmedia.ReportGenerator.Parser.Analysis
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
        /// <param name="line">The line number.</param>
        internal CodeElement(string name, CodeElementType type, int line)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.CodeElementType = type;
            this.Line = line;
        }

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
        /// Gets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int Line { get; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
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
                return this.Name.Equals(codeElement.Name) && this.Line == codeElement.Line;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => this.Name.GetHashCode() + this.Line.GetHashCode();
    }
}
