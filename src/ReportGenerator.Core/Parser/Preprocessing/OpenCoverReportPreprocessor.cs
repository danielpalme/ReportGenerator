using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for OpenCover reports.
    /// </summary>
    internal class OpenCoverReportPreprocessor
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
                .Elements("Classes")
                .Elements("Class")
                .Where(c => c.Element("FullName").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase)
                    && c.Element("FullName").Value.Contains("/"))
                .ToArray();

            var classesInModule = module
                .Elements("Classes")
                .Elements("Class")
                .Where(c => !c.Element("FullName").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (var startupCodeClass in startupCodeClasses)
            {
                var methods = startupCodeClass
                    .Elements("Methods")
                    .Elements("Method")
                    .Where(c => c.Element("FileRef") != null)
                    .ToArray();

                var fileIds = methods.Elements("FileRef")
                    .Select(e => e.Attribute("uid").Value)
                    .Distinct()
                    .ToArray();

                if (fileIds.Length != 1)
                {
                    continue;
                }

                var lineNumbers = methods
                    .Elements("SequencePoints")
                    .Elements("SequencePoint")
                    .Where(s => s.Attribute("sl") != null)
                    .Select(s => int.Parse(s.Attribute("sl").Value, CultureInfo.InvariantCulture))
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
                    var methodsOfClass = @class
                        .Elements("Methods")
                        .Elements("Method")
                        .Where(c => c.Element("FileRef") != null)
                        .ToArray();

                    var fileIdsOfClass = methodsOfClass
                        .Elements("FileRef")
                        .Select(e => e.Attribute("uid").Value)
                        .Distinct()
                        .ToArray();

                    if (fileIdsOfClass.Length != 1 || fileIdsOfClass[0] != fileIds[0])
                    {
                        continue;
                    }

                    var lineNumbersOfClass = methodsOfClass
                        .Elements("SequencePoints")
                        .Elements("SequencePoint")
                        .Where(s => s.Attribute("sl") != null)
                        .Select(s => int.Parse(s.Attribute("sl").Value, CultureInfo.InvariantCulture))
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
                    startupCodeClass.Element("FullName").Value = closestClass.Element("FullName").Value + "/" + startupCodeClass.Element("FullName").Value;
                }
            }
        }
    }
}
