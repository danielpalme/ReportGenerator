// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "danielpalme", Scope = "resource", Target = "Palmmedia.ReportGenerator.Properties.Resources.resources", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "github", Scope = "resource", Target = "Palmmedia.ReportGenerator.Properties.Resources.resources", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Common", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.MSBuild", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Parser", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Parser.Analysis", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Reporting", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Reporting.Rendering", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Palmmedia", Scope = "namespace", Target = "Palmmedia.ReportGenerator.Logging", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Coverages", Scope = "member", Target = "Palmmedia.ReportGenerator.Parser.Analysis.Class.#HistoricCoverages", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type", Target = "Palmmedia.ReportGenerator.MSBuild.ReportGenerator", Justification = "MsBuild task should have the same name.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "Palmmedia.ReportGenerator.ReportConfigurationBuilder.#ShowHelp()", Justification = "This is just for layout.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Palmmedia.ReportGenerator.Logging.ILogger.Error(System.String)", Scope = "member", Target = "Palmmedia.ReportGenerator.Parser.Analysis.CodeFile.#AnalyzeFile()", Justification = "String does not contain any text.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Palmmedia.ReportGenerator.Logging.ILogger.Debug(System.String)", Scope = "member", Target = "Palmmedia.ReportGenerator.Parser.ParserFactory.#GetParsersOfFile(System.String,Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch.ClassSearcherFactory,Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch.ClassSearcher)", Justification = "String does not contain any text.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Scope = "member", Target = "Palmmedia.ReportGenerator.ReportConfigurationBuilder.#ShowHelp()", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", Scope = "resource", Target = "Palmmedia.ReportGenerator.Properties.Help.resources", Justification = "Spelling ist valid.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Palmmedia.ReportGenerator.Reporting.MefReportBuilderFactory.#LoadReportBuilders()", Justification = "Catalogs are disposed when CompositionContainer gets disposed.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Scope = "member", Target = "Palmmedia.ReportGenerator.Reporting.MefReportBuilderFactory.#LoadReportBuilders()", Justification = "Assemblies have to get loaded to support plugins.")]
