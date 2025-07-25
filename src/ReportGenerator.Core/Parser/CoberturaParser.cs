using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Parser for XML reports generated by Cobertura.
    /// </summary>
    internal class CoberturaParser : ParserBase
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CoberturaParser));

        /// <summary>
        /// Regex to analyze if a method name belongs to a lamda expression.
        /// </summary>
        private static readonly Regex LambdaMethodNameRegex = new Regex("<.+>.+__", RegexOptions.Compiled);

        /// <summary>
        /// Regex to analyze if a method name is generated by compiler.
        /// </summary>
        private static readonly Regex CompilerGeneratedMethodNameRegex = new Regex(@"(?<ClassName>.+)(/|\.)<(?<CompilerGeneratedName>.+)>.+__.+MoveNext\(\)$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to analyze if a method name is a nested method (a method nested within a method).
        /// </summary>
        private static readonly Regex LocalFunctionMethodNameRegex = new Regex(@"^.*(?<ParentMethodName><.+>).*__(?<NestedMethodName>[^\|]+)\|.*$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to analyze the branch coverage of a line element.
        /// </summary>
        private static readonly Regex BranchCoverageRegex = new Regex("\\((?<NumberOfCoveredBranches>\\d+)/(?<NumberOfTotalBranches>\\d+)\\)$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="CoberturaParser" /> class.
        /// </summary>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        internal CoberturaParser(IFilter assemblyFilter, IFilter classFilter, IFilter fileFilter)
            : base(assemblyFilter, classFilter, fileFilter)
        {
        }

        /// <summary>
        /// Parses the given XML report.
        /// </summary>
        /// <param name="report">The XML report.</param>
        /// <returns>The parser result.</returns>
        public ParserResult Parse(XContainer report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var assemblies = new List<Assembly>();

            var assemblyElementGrouping = report.Descendants("package")
                .GroupBy(m => m.Attribute("name").Value)
                .Where(a => this.AssemblyFilter.IsElementIncludedInReport(a.Key))
                .ToArray();

            foreach (var elements in assemblyElementGrouping)
            {
                assemblies.Add(this.ProcessAssembly(elements.ToArray(), elements.Key));
            }

            var result = new ParserResult(assemblies.OrderBy(a => a.Name).ToList(), true, this.ToString());

            foreach (var sourceElement in report.Elements("sources").Elements("source"))
            {
                result.AddSourceDirectory(sourceElement.Value);
            }

            try
            {
                if (report.Element("sources")?.Parent.Attribute("timestamp") != null)
                {
                    DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    timeStamp = timeStamp.AddSeconds(double.Parse(report.Element("sources").Parent.Attribute("timestamp").Value)).ToLocalTime();

                    result.MinimumTimeStamp = timeStamp;
                    result.MaximumTimeStamp = timeStamp;
                }
            }
            catch (Exception)
            {
                // Ignore since timestamp is not relevant. If timestamp is missing or in wrong format the information is just missing in the report(s)
            }

            return result;
        }

        /// <summary>
        /// Processes the given assembly.
        /// </summary>
        /// <param name="modules">The modules.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>The <see cref="Assembly"/>.</returns>
        private Assembly ProcessAssembly(XElement[] modules, string assemblyName)
        {
            Logger.DebugFormat(Resources.CurrentAssembly, assemblyName);

            var classes = modules
                .Elements("classes")
                .Elements("class")
                .ToArray();

            var classNames = classes
                .Select(c => ClassNameParser.ParseClassName(c.Attribute("name").Value, this.RawMode))
                .Where(c => c.Include)
                .Distinct()
                .Where(c => this.ClassFilter.IsElementIncludedInReport(c.Name))
                .OrderBy(c => c.Name)
                .ToArray();

            var assembly = new Assembly(assemblyName);

            Parallel.ForEach(classNames, c => this.ProcessClass(classes, assembly, c));

            return assembly;
        }

        /// <summary>
        /// Processes the given class.
        /// </summary>
        /// <param name="allClasses">All class elements.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="classNameParserResult">Name of the class.</param>
        private void ProcessClass(XElement[] allClasses, Assembly assembly, ClassNameParserResult classNameParserResult)
        {
            bool FilterClass(XElement element)
            {
                var name = element.Attribute("name").Value;

                return name.Equals(classNameParserResult.Name)
                    || (!this.RawMode
                        && name.StartsWith(classNameParserResult.Name, StringComparison.Ordinal)
                        && (name[classNameParserResult.Name.Length] == '$'
                            || name[classNameParserResult.Name.Length] == '/'
                            || name[classNameParserResult.Name.Length] == '.'));
            }

            var classes = allClasses
                .Where(FilterClass)
                .ToArray();

            var files = classes
                .Select(c => c.Attribute("filename").Value)
                .Distinct()
                .ToArray();

            var filteredFiles = files
                .Where(f => this.FileFilter.IsElementIncludedInReport(f))
                .ToArray();

            // If all files are removed by filters, then the whole class is omitted
            if ((files.Length == 0 && !this.FileFilter.HasCustomFilters) || filteredFiles.Length > 0)
            {
                var @class = new Class(classNameParserResult.DisplayName, classNameParserResult.RawName, assembly);

                foreach (var file in filteredFiles)
                {
                    var fileClasses = classes
                        .Where(c => c.Attribute("filename").Value.Equals(file))
                        .ToArray();
                    @class.AddFile(this.ProcessFile(fileClasses, @class, classNameParserResult.Name, file));
                }

                assembly.AddClass(@class);
            }
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="classElements">The class elements for the file.</param>
        /// <param name="class">The class.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>The <see cref="CodeFile"/>.</returns>
        private CodeFile ProcessFile(XElement[] classElements, Class @class, string className, string filePath)
        {
            var lines = classElements.Elements("lines")
                .Elements("line")
                .ToArray();

            var lineNumbers = lines
                .Select(l => l.Attribute("number").Value)
                .ToHashSet();

            var methodsOfFile = classElements
                .Elements("methods")
                .Elements("method")
                .ToArray();

            var additionalLinesInMethodElement = methodsOfFile
                .Elements("lines")
                .Elements("line")
                .Where(l => !lineNumbers.Contains(l.Attribute("number").Value))
                .ToArray();

            var linesOfFile = lines.Concat(additionalLinesInMethodElement)
                .Select(line => new
                {
                    LineNumber = int.Parse(line.Attribute("number").Value, CultureInfo.InvariantCulture),
                    Visits = line.Attribute("hits").Value.ParseLargeInteger()
                })
                .OrderBy(seqpnt => seqpnt.LineNumber)
                .ToArray();

            var branches = GetBranches(lines);

            int[] coverage = new int[] { };
            LineVisitStatus[] lineVisitStatus = new LineVisitStatus[] { };

            if (linesOfFile.Length > 0)
            {
                coverage = new int[linesOfFile[linesOfFile.LongLength - 1].LineNumber + 1];
                lineVisitStatus = new LineVisitStatus[linesOfFile[linesOfFile.LongLength - 1].LineNumber + 1];

                for (int i = 0; i < coverage.Length; i++)
                {
                    coverage[i] = -1;
                }

                foreach (var line in linesOfFile)
                {
                    coverage[line.LineNumber] = line.Visits;

                    bool partiallyCovered = false;

                    if (branches.TryGetValue(line.LineNumber, out ICollection<Branch> branchesOfLine))
                    {
                        partiallyCovered = branchesOfLine.Any(b => b.BranchVisits == 0);
                    }

                    LineVisitStatus statusOfLine = line.Visits > 0 ? (partiallyCovered ? LineVisitStatus.PartiallyCovered : LineVisitStatus.Covered) : LineVisitStatus.NotCovered;
                    lineVisitStatus[line.LineNumber] = statusOfLine;
                }
            }

            var codeFile = new CodeFile(filePath, coverage, lineVisitStatus, branches);

            this.SetMethodMetrics(codeFile, methodsOfFile);
            this.SetCodeElements(codeFile, methodsOfFile);

            return codeFile;
        }

        /// <summary>
        /// Extracts the metrics from the given <see cref="XElement">XElements</see>.
        /// </summary>
        /// <param name="codeFile">The code file.</param>
        /// <param name="methodsOfFile">The methods of the file.</param>
        private void SetMethodMetrics(CodeFile codeFile, IEnumerable<XElement> methodsOfFile)
        {
            foreach (var method in methodsOfFile)
            {
                string fullName = method.Attribute("name").Value + method.Attribute("signature").Value;
                string methodName = this.ExtractMethodName(fullName, method.Parent.Parent.Attribute("name").Value);

                if (!this.RawMode && methodName.Contains("__") && LambdaMethodNameRegex.IsMatch(methodName))
                {
                    continue;
                }

                string shortName = GetShortMethodName(methodName);

                var metrics = new List<Metric>();

                var lineRate = method.Attribute("line-rate");

                decimal? coveragePercent = null;
                if (lineRate != null)
                {
                    coveragePercent = GetCoberturaDecimalPercentageValue(lineRate.Value);

                    metrics.Add(Metric.Coverage(coveragePercent));
                }

                var branchRate = method.Attribute("branch-rate");

                if (branchRate != null)
                {
                    decimal? value = GetCoberturaDecimalPercentageValue(branchRate.Value);

                    metrics.Add(Metric.BranchCoverage(value));
                }

                var cyclomaticComplexityAttribute = method.Attribute("complexity");
                decimal? cyclomaticComplexity = null;
                if (cyclomaticComplexityAttribute != null)
                {
                    cyclomaticComplexity = GetCoberturaDecimalValue(cyclomaticComplexityAttribute.Value);

                    metrics.Insert(
                        0,
                        Metric.CyclomaticComplexity(cyclomaticComplexity));
                }

                if (cyclomaticComplexity.HasValue && coveragePercent.HasValue)
                {
                    // https://testing.googleblog.com/2011/02/this-code-is-crap.html
                    // CRAP(m) = CC(m)^2 * U(m)^3 + CC(m)
                    // CC(m) <= Cyclomatic Complexity (e.g. 5)
                    // U(m) <= Uncovered percentage (e.g. 30% = 0.3)
                    var uncoveredPercent = (100f - (double)coveragePercent.Value) / 100.0;
                    var complexity = (double)cyclomaticComplexity.Value;
                    var crapScore = (Math.Pow(complexity, 2.0) * Math.Pow(uncoveredPercent, 3)) + complexity;
                    crapScore = Math.Round(crapScore, 0, MidpointRounding.AwayFromZero);

                    metrics.Insert(0, Metric.CrapScore((decimal)crapScore));
                }

                var methodMetric = new MethodMetric(methodName, shortName, metrics);

                var line = method
                    .Elements("lines")
                    .Elements("line")
                    .FirstOrDefault();

                if (line != null)
                {
                    methodMetric.Line = int.Parse(line.Attribute("number").Value, CultureInfo.InvariantCulture);
                }

                codeFile.AddMethodMetric(methodMetric);
            }
        }

        private static decimal? ParseCoberturaDecimalValue(string value)
        {
            decimal? result = null;
            if (!"NaN".Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = decimal.Parse(value.Replace(',', '.'), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            }

            return result;
        }

        private static decimal? GetCoberturaDecimalValue(string value)
        {
            decimal? result = ParseCoberturaDecimalValue(value);
            if (result.HasValue)
            {
                result = Math.Round(result.Value, 2, MidpointRounding.AwayFromZero);
            }

            return result;
        }

        private static decimal? GetCoberturaDecimalPercentageValue(string value)
        {
            decimal? result = ParseCoberturaDecimalValue(value);
            if (result.HasValue)
            {
                result = Math.Round(result.Value * 100, 2, MidpointRounding.AwayFromZero);
            }

            return result;
        }

        /// <summary>
        /// Gets the branches by line number.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <returns>The branches by line number.</returns>
        private static Dictionary<int, ICollection<Branch>> GetBranches(IEnumerable<XElement> lines)
        {
            var result = new Dictionary<int, ICollection<Branch>>();

            foreach (var line in lines)
            {
                if (line.Attribute("condition-coverage") == null
                    || line.Attribute("branch") == null
                    || !line.Attribute("branch").Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var match = BranchCoverageRegex.Match(line.Attribute("condition-coverage").Value);

                if (match.Success)
                {
                    int lineNumber = int.Parse(line.Attribute("number").Value, CultureInfo.InvariantCulture);

                    int numberOfCoveredBranches = match.Groups["NumberOfCoveredBranches"].Value.ParseLargeInteger();
                    int numberOfTotalBranches = match.Groups["NumberOfTotalBranches"].Value.ParseLargeInteger();

                    var branches = new HashSet<Branch>();

                    for (int i = 0; i < numberOfTotalBranches; i++)
                    {
                        string identifier = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}_{1}",
                            lineNumber,
                            i);

                        branches.Add(new Branch(i < numberOfCoveredBranches ? 1 : 0, identifier));
                    }

                    /* If cobertura file is merged from different files, a class and therefore each line can exist several times.
                     * The best result is used. */
                    if (result.TryGetValue(lineNumber, out ICollection<Branch> existingBranches))
                    {
                        if (numberOfCoveredBranches > existingBranches.Count(b => b.BranchVisits == 1))
                        {
                            result[lineNumber] = branches;
                        }
                    }
                    else
                    {
                        result.Add(lineNumber, branches);
                    }
                }
            }

            return result;
        }

        private static string GetShortMethodName(string fullName)
        {
            int indexOpen = fullName.IndexOf('(');

            if (indexOpen <= 0)
            {
                return fullName;
            }

            int indexClose = fullName.IndexOf(')');
            string signature = indexClose - indexOpen > 1 ? "(...)" : "()";

            return $"{fullName.Substring(0, indexOpen)}{signature}";
        }

        /// <summary>
        /// Extracts the methods/properties of the given <see cref="XElement">XElements</see>.
        /// </summary>
        /// <param name="codeFile">The code file.</param>
        /// <param name="methodsOfFile">The methods of the file.</param>
        private void SetCodeElements(CodeFile codeFile, IEnumerable<XElement> methodsOfFile)
        {
            foreach (var method in methodsOfFile)
            {
                string fullName = method.Attribute("name").Value + method.Attribute("signature").Value;
                string methodName = this.ExtractMethodName(fullName, method.Parent.Parent.Attribute("name").Value);

                if (!this.RawMode && methodName.Contains("__") && LambdaMethodNameRegex.IsMatch(methodName))
                {
                    continue;
                }

                var lines = method.Elements("lines")
                    .Elements("line");

                if (lines.Any())
                {
                    int firstLine = int.Parse(lines.First().Attribute("number").Value, CultureInfo.InvariantCulture);
                    int lastLine = int.Parse(lines.Last().Attribute("number").Value, CultureInfo.InvariantCulture);

                    codeFile.AddCodeElement(new CodeElement(
                        methodName,
                        methodName,
                        methodName.StartsWith("get_") || methodName.StartsWith("set_") ? CodeElementType.Property : CodeElementType.Method,
                        firstLine,
                        lastLine,
                        codeFile.CoverageQuotaInRange(firstLine, lastLine)));
                }
            }
        }

        /// <summary>
        /// Extracts the method name. For async methods the original name is returned.
        /// </summary>
        /// <param name="methodName">The full method name.</param>
        /// <param name="className">The name of the class.</param>
        /// <returns>The method name.</returns>
        private string ExtractMethodName(string methodName, string className)
        {
            if (this.RawMode)
            {
                return methodName;
            }

            if (methodName.Contains("|") || className.Contains("|"))
            {
                Match match = LocalFunctionMethodNameRegex.Match(className + methodName);

                if (match.Success)
                {
                    methodName = match.Groups["NestedMethodName"].Value + "()";
                }
            }
            else if (methodName.EndsWith("MoveNext()"))
            {
                Match match = CompilerGeneratedMethodNameRegex.Match(className + methodName);

                if (match.Success)
                {
                    methodName = match.Groups["CompilerGeneratedName"].Value + "()";
                }
            }

            return methodName;
        }
    }
}
