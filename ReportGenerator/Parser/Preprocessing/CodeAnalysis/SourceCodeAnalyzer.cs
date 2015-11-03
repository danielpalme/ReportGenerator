using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.CodeAnalysis
{
    /// <summary>
    /// Helper class to determine the begin and end line number of source code elements within a source code file.
    /// </summary>
    internal static class SourceCodeAnalyzer
    {
        /// <summary>
        /// The name of the last source code file that has successfully been parsed.
        /// </summary>
        private static string lastFilename;

        /// <summary>
        /// The <see cref="AstNode"/> of the last source code file that has successfully been parsed.
        /// </summary>
        private static AstNode lastNode;

        /// <summary>
        /// Gets all classes in the given file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>All classes (with full namespace).</returns>
        internal static IEnumerable<string> GetClassesInFile(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            AstNode parentNode = GetParentNode(filename);

            if (parentNode == null)
            {
                return new string[] { };
            }
            else
            {
                return FindClasses(new AstNode[] { parentNode }).Select(c => GetFullClassName(c)).Distinct();
            }
        }

        /// <summary>
        /// Searches the given source code file for a source element matching the given <see cref="SourceElement"/>.
        /// If the source element can be found, a <see cref="SourceElementPosition"/> containing the start and end line numbers is returned.
        /// Otherwise <c>null</c> is returned.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="sourceElement">The source element.</param>
        /// <returns>A <see cref="SourceElementPosition"/> or <c>null</c> if source element can not be found.</returns>
        internal static SourceElementPosition FindSourceElement(string filename, SourceElement sourceElement)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (sourceElement == null)
            {
                throw new ArgumentNullException(nameof(sourceElement));
            }

            AstNode parentNode = GetParentNode(filename);

            if (parentNode == null)
            {
                return null;
            }
            else
            {
                var matchingClasses = FindClasses(new AstNode[] { parentNode }).Where(c => GetFullClassName(c) == sourceElement.Classname);

                return FindSourceElement(matchingClasses, sourceElement);
            }
        }

        /// <summary>
        /// Searches the given <see cref="ICSharpCode.NRefactory.PatternMatching.INode">INodes</see> recursively for classes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>The type declarations corresponding to all classes.</returns>
        private static IEnumerable<TypeDeclaration> FindClasses(IEnumerable<AstNode> nodes)
        {
            var result = new List<TypeDeclaration>();

            foreach (var node in nodes)
            {
                var @class = node as TypeDeclaration;
                if (@class != null)
                {
                    result.Add(@class);
                }

                if (@class != null || node is NamespaceDeclaration || node is SyntaxTree)
                {
                    result.AddRange(FindClasses(node.Children));
                }
            }

            return result;
        }

        /// <summary>
        /// Searches the given <see cref="ICSharpCode.NRefactory.PatternMatching.INode">INodes</see> recursively for the given <see cref="SourceElement"/>.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="sourceElement">The source element.</param>
        /// <returns>A <see cref="SourceElementPosition"/> or <c>null</c> if source element can not be found.</returns>
        private static SourceElementPosition FindSourceElement(IEnumerable<AstNode> nodes, SourceElement sourceElement)
        {
            foreach (var node in nodes)
            {
                var sourceElementPosition = sourceElement.GetSourceElementPosition(node);
                if (sourceElementPosition != null)
                {
                    return sourceElementPosition;
                }
            }

            foreach (var node in nodes)
            {
                var sourceElementPosition = FindSourceElement(node.Children, sourceElement);
                if (sourceElementPosition != null)
                {
                    return sourceElementPosition;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the topmost <see cref="ICSharpCode.NRefactory.PatternMatching.INode"/> in the given source file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The topmost <see cref="ICSharpCode.NRefactory.PatternMatching.INode"/> in the given source file.</returns>
        private static AstNode GetParentNode(string filename)
        {
            if (filename.Equals(lastFilename))
            {
                return lastNode;
            }
            else
            {
                try
                {
                    using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        var parser = new CSharpParser();

                        var syntaxTree = parser.Parse(fs, filename);

                        if (parser.HasErrors || syntaxTree == null)
                        {
                            return null;
                        }

                        // Cache the node
                        lastFilename = filename;
                        lastNode = syntaxTree;

                        return lastNode;
                    }
                }
                catch (System.IO.IOException)
                {
                    return null;
                }
                catch (InvalidCastException)
                {
                    // NRefactory does not support .NET 4.6 yet
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the full name of the class.
        /// </summary>
        /// <param name="typeDeclaration">The type declaration.</param>
        /// <returns>The full name of the class.</returns>
        private static string GetFullClassName(TypeDeclaration typeDeclaration)
        {
            string result = typeDeclaration.Name;

            AstNode current = typeDeclaration;
            while (current.Parent != null)
            {
                current = current.Parent;

                var parentTypeDeclaration = current as TypeDeclaration;
                var parentNamespaceDeclaration = current as NamespaceDeclaration;

                if (parentTypeDeclaration != null)
                {
                    result = parentTypeDeclaration.Name + result;
                }
                else if (parentNamespaceDeclaration != null)
                {
                    result = parentNamespaceDeclaration.Name + "." + result;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
