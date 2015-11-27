using System;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.CodeAnalysis
{
    /// <summary>
    /// Represents a property in a source file.
    /// </summary>
    internal class PropertyElement : SourceElement
    {
        /// <summary>
        /// Prefix of GET property.
        /// </summary>
        private const string GetterPrefix = "get_";

        /// <summary>
        /// Prefix of SET property.
        /// </summary>
        private const string SetterPrefix = "set_";

        /// <summary>
        /// The name of the property.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyElement"/> class.
        /// </summary>
        /// <param name="classname">The classname.</param>
        /// <param name="name">The name of the property.</param>
        internal PropertyElement(string classname, string name)
            : base(classname)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.name = name;

            // Cut off prefix only if property starts with standard getter ("get_") / setter prefix ("set_")
            if (this.name.StartsWith(GetterPrefix, StringComparison.Ordinal))
            {
                this.name = name.Substring(GetterPrefix.Length);
            }
            else if (this.name.StartsWith(SetterPrefix, StringComparison.Ordinal))
            {
                this.name = name.Substring(SetterPrefix.Length);
            }
        }

        /// <summary>
        /// Determines whether the given <see cref="ICSharpCode.NRefactory.PatternMatching.INode"/> matches the <see cref="SourceElement"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// A <see cref="SourceElementPosition"/> or <c>null</c> if <see cref="SourceElement"/> does not match the <see cref="ICSharpCode.NRefactory.PatternMatching.INode"/>.
        /// </returns>
        internal override SourceElementPosition GetSourceElementPosition(INode node)
        {
            PropertyDeclaration propertyDeclaration = node as PropertyDeclaration;

            if (propertyDeclaration != null && propertyDeclaration.Name.Equals(this.name))
            {
                return new SourceElementPosition(
                    propertyDeclaration.StartLocation.Line,
                    propertyDeclaration.EndLocation.Line);
            }

            return null;
        }
    }
}
