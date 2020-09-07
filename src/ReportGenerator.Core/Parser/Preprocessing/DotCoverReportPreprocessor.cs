using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for OpenCover reports.
    /// </summary>
    internal class DotCoverReportPreprocessor
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(DotCoverReportPreprocessor));

        /// <summary>
        /// Executes the preprocessing of the report.
        /// </summary>
        /// <param name="report">The report.</param>
        internal void Execute(XContainer report)
        {
            bool statementExists = report.Descendants("Assembly")
               .Elements("Namespace")
               .Elements("Type")
               .Descendants("Method")
               .Elements("Statement")
               .Any();

            if (!statementExists)
            {
                Logger.Error(Resources.ErrorInvalidDotCoverReport);
            }

            foreach (var module in report.Descendants("Assembly").ToArray())
            {
                MoveStartupCodeElementsToParentType(module);
            }
        }

        /// <summary>
        /// Moves startup code elements to the parent class.
        /// </summary>
        /// <param name="module">The module.</param>
        private static void MoveStartupCodeElementsToParentType(XElement module)
        {
            var startupCodeModules = module
                .Elements("Namespace")
                .Where(c => c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .Where(t => t.Attribute("Name").Value.StartsWith("$Module", StringComparison.OrdinalIgnoreCase));

            var startupCodeClasses = startupCodeModules
                .Elements("Type")
                .ToArray();

            var classesInModule = module
                .Elements("Namespace")
                .Where(c => !c.Attribute("Name").Value.StartsWith("<StartupCode$", StringComparison.OrdinalIgnoreCase))
                .Elements("Type")
                .ToArray();

            foreach (var startupCodeClass in startupCodeClasses)
            {
                var statements = startupCodeClass
                    .Descendants("Statement")
                    .ToArray();

                var fileIds = statements
                    .Select(s => s.Attribute("FileIndex").Value)
                    .Distinct()
                    .ToArray();

                if (fileIds.Length != 1)
                {
                    continue;
                }

                var lineNumbers = statements
                    .Select(s => int.Parse(s.Attribute("Line").Value, CultureInfo.InvariantCulture))
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
                    var statementsOfClass = @class
                        .Descendants("Statement")
                        .ToArray();

                    var fileIdsOfClass = statementsOfClass
                        .Select(s => s.Attribute("FileIndex").Value)
                        .Distinct()
                        .ToArray();

                    if (fileIdsOfClass.Length != 1 || fileIdsOfClass[0] != fileIds[0])
                    {
                        continue;
                    }

                    var lineNumbersOfClass = statementsOfClass
                        .Select(s => int.Parse(s.Attribute("Line").Value, CultureInfo.InvariantCulture))
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
                    startupCodeClass.Remove();
                    closestClass.Add(startupCodeClass);
                }
            }

            foreach (var startupCodeModule in startupCodeModules.ToArray())
            {
                startupCodeModule.Remove();
            }
        }
    }
}