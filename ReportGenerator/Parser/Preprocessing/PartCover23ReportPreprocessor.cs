using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Preprocessing.CodeAnalysis;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing
{
    /// <summary>
    /// Preprocessor for PartCover 2.3 reports.
    /// </summary>
    internal class PartCover23ReportPreprocessor : ReportPreprocessorBase
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(PartCover23ReportPreprocessor));

        /// <summary>
        /// Initializes a new instance of the <see cref="PartCover23ReportPreprocessor"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="classSearcherFactory">The class searcher factory.</param>
        /// <param name="globalClassSearcher">The global class searcher.</param>
        internal PartCover23ReportPreprocessor(XContainer report, ClassSearcherFactory classSearcherFactory, ClassSearcher globalClassSearcher)
            : base(report, classSearcherFactory, globalClassSearcher)
        {
        }

        /// <summary>
        /// Executes the preprocessing of the report.
        /// </summary>
        internal override void Execute()
        {
            var filenameByFileIdDictionary = this.Report
                .Descendants("File")
                .ToDictionary(f => f.Attribute("id").Value, f => f.Attribute("url").Value);

            this.AddCoverageDataOfUnexecutedMethods(filenameByFileIdDictionary);
            this.AddCoverageDataOfAutoProperties(filenameByFileIdDictionary);
        }

        /// <summary>
        /// Adds a new source code file to the report.
        /// </summary>
        /// <param name="filesContainer">The files container.</param>
        /// <param name="fileId">The file id.</param>
        /// <param name="file">The file path.</param>
        protected override void AddNewFile(XContainer filesContainer, string fileId, string file)
        {
            XDocument document = filesContainer as XDocument;

            if (document != null)
            {
                document.Root.Add(new XElement("File", new XAttribute("id", fileId), new XAttribute("url", file)));
            }
            else
            {
                filesContainer.Add(new XElement("File", new XAttribute("id", fileId), new XAttribute("url", file)));
            }
        }

        /// <summary>
        /// Updates the method element.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="elementPosition">The element position.</param>
        /// <param name="fileId">The file id.</param>
        private static void UpdateMethodElement(XContainer method, SourceElementPosition elementPosition, string fileId)
        {
            for (int i = elementPosition.Start; i <= elementPosition.End; i++)
            {
                var seqpnt = new XElement(
                    "pt",
                    new XAttribute("visit", "0"),
                    new XAttribute("fid", fileId),
                    new XAttribute("sl", i));

                method.Add(seqpnt);
            }
        }

        /// <summary>
        /// Updates the property element.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="elementPosition">The element position.</param>
        /// <param name="fileId">The file id.</param>
        private static void UpdatePropertyElement(XContainer property, SourceElementPosition elementPosition, string fileId)
        {
            if (!property.Elements().Any())
            {
                var seqpnt = new XElement(
                    "pt",
                    new XAttribute("visit", "0"),
                    new XAttribute("fid", fileId),
                    new XAttribute("sl", elementPosition.Start));

                property.Add(seqpnt);
            }
            else
            {
                foreach (var pt in property.Elements().Take(1))
                {
                    pt.Add(new XAttribute("sl", elementPosition.Start));
                    pt.Add(new XAttribute("fid", fileId));
                }
            }
        }

        /// <summary>
        /// Adds the coverage data of unexecuted methods.
        /// </summary>
        /// <param name="filenameByFileIdDictionary">Dictionary containing all files used in the report by their corresponding id.</param>
        private void AddCoverageDataOfUnexecutedMethods(Dictionary<string, string> filenameByFileIdDictionary)
        {
            var unexecutedMethods = this.Report.Descendants("Type")
                .Where(type => !type.Attribute("name").Value.Contains("__"))
                .Elements("Method")
                .Where(m => !m.Attribute("name").Value.StartsWith("get_", StringComparison.Ordinal) && !m.Attribute("name").Value.StartsWith("set_", StringComparison.Ordinal))
                .Where(m => !m.Elements().Any())
                .ToArray();

            long counter = 0;
            foreach (var method in unexecutedMethods)
            {
                PartCoverMethodElement partCoverMethodElement = new PartCoverMethodElement(
                    method.Parent.Attribute("name").Value,
                    method.Attribute("name").Value,
                    method.Attribute("sig").Value);

                // Get files in which property could be defined
                var fileIds = method.Parent.Descendants("pt")
                    .Where(p => p.Attribute("fid") != null)
                    .Select(p => p.Attribute("fid").Value)
                    .Distinct()
                    .ToArray();

                if (this.SearchElement(
                    partCoverMethodElement,
                    filenameByFileIdDictionary,
                    fileIds,
                    method,
                    UpdateMethodElement,
                    this.Report))
                {
                    counter++;
                }
            }

            if (unexecutedMethods.LongLength > 0)
            {
                Logger.DebugFormat("  " + Resources.AddedCoverageInformationUnexecutedMethods, counter, unexecutedMethods.LongLength);
            }
        }

        /// <summary>
        /// Adds the coverage data of auto properties.
        /// </summary>
        /// <param name="filenameByFileIdDictionary">Dictionary containing all files used in the report by their corresponding id.</param>
        private void AddCoverageDataOfAutoProperties(Dictionary<string, string> filenameByFileIdDictionary)
        {
            Func<string, bool> isProperty = v => v.StartsWith("get_", StringComparison.Ordinal) || v.StartsWith("set_", StringComparison.Ordinal);

            var unexecutedProperties = this.Report.Descendants("Type")
                .Where(type => !type.Attribute("name").Value.Contains("__"))
                .Elements("Method")
                .Where(m => isProperty(m.Attribute("name").Value)
                    && (!m.Elements().Any() || !m.Elements().Any(pt => pt.Attribute("sl") != null)))
                .ToArray();

            long counter = 0;
            foreach (var property in unexecutedProperties)
            {
                var propertyElement = new PropertyElement(property.Parent.Attribute("name").Value, property.Attribute("name").Value);

                // Get files in which property could be defined
                var fileIds = property.Parent.Descendants("pt")
                    .Where(p => p.Attribute("fid") != null)
                    .Select(p => p.Attribute("fid").Value)
                    .Distinct()
                    .ToArray();

                if (this.SearchElement(
                    propertyElement,
                    filenameByFileIdDictionary,
                    fileIds,
                    property,
                    UpdatePropertyElement,
                    this.Report))
                {
                    counter++;
                }
            }

            if (unexecutedProperties.LongLength > 0)
            {
                Logger.DebugFormat("  " + Resources.AddedCoverageInformationOfProperties, counter, unexecutedProperties.LongLength);
            }
        }
    }
}
