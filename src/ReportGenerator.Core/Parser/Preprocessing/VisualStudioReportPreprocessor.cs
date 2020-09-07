using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for Visual Studio reports.
    /// </summary>
    internal class VisualStudioReportPreprocessor
    {
        /// <summary>
        /// Executes the preprocessing of the report.
        /// </summary>
        /// <param name="report">The report.</param>
        internal void Execute(XContainer report)
        {
            foreach (var module in report.Descendants("Module").ToArray())
            {
                ApplyClassNameToStartupCodeElements(module);
            }
        }

        /// <summary>
        /// Applies the class name of the parent class to startup code elements.
        /// </summary>
        /// <param name="module">The module.</param>
        private static void ApplyClassNameToStartupCodeElements(XElement module)
        {
            var startupCodeClasses = module
                .Elements("NamespaceTable")
                .Where(c => c.Element("NamespaceName").Value.StartsWith("<StartupCode$", System.StringComparison.OrdinalIgnoreCase))
                .Elements("Class")
                .Where(c => c.Element("ClassName").Value.Contains("."))
                .ToArray();

            var classesInModule = module
                .Elements("NamespaceTable")
                .Where(c => !c.Element("NamespaceName").Value.StartsWith("<StartupCode$", System.StringComparison.OrdinalIgnoreCase))
                .Elements("Class")
                .ToArray();

            foreach (var startupCodeClass in startupCodeClasses)
            {
                var fileIds = startupCodeClass
                    .Elements("Method")
                    .Elements("Lines")
                    .Elements("SourceFileID")
                    .Select(e => e.Value)
                    .Distinct()
                    .ToArray();

                if (fileIds.Length != 1)
                {
                    continue;
                }

                var lineNumbers = startupCodeClass
                    .Elements("Method")
                    .Elements("Lines")
                    .Elements("LnStart")
                    .Select(s => int.Parse(s.Value, CultureInfo.InvariantCulture))
                    .OrderBy(v => v)
                    .Take(1)
                    .ToArray();

                if (lineNumbers.Length != 1)
                {
                    continue;
                }

                XElement closestClass = null;
                int closestLineNumber = 0;

                foreach (var @class in classesInModule)
                {
                    var linesOfClass = @class
                        .Elements("Method")
                        .Elements("Lines")
                        .ToArray();

                    var fileIdsOfClass = linesOfClass
                        .Elements("SourceFileID")
                        .Select(e => e.Value)
                        .Distinct()
                        .ToArray();

                    if (fileIdsOfClass.Length != 1 || fileIdsOfClass[0] != fileIds[0])
                    {
                        continue;
                    }

                    var lineNumbersOfClass = linesOfClass
                        .Elements("LnStart")
                        .Select(s => int.Parse(s.Value, CultureInfo.InvariantCulture))
                        .OrderBy(v => v)
                        .Take(1)
                        .ToArray();

                    /* Conditions:
                        * 1) No line numbers available
                        * 2) Class comes after current class
                        * 3) Closer class has already been found */
                    if (lineNumbersOfClass.Length != 1
                        || lineNumbersOfClass[0] > lineNumbers[0]
                        || closestLineNumber > lineNumbersOfClass[0])
                    {
                        continue;
                    }
                    else
                    {
                        closestClass = @class;
                        closestLineNumber = lineNumbersOfClass[0];
                    }
                }

                if (closestClass != null)
                {
                    startupCodeClass.Parent.Element("NamespaceName").Value = closestClass.Parent.Element("NamespaceName").Value;
                    startupCodeClass.Element("ClassName").Value = closestClass.Element("ClassName").Value;
                }
            }
        }
    }
}
