// In: internal/reporter/textsummary/reporter.go
package textsummary

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"text/tabwriter"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/reporter"
)

// TextReportBuilder generates a text summary report.
type TextReportBuilder struct {
	outputDir string
}

// NewTextReportBuilder creates a new TextReportBuilder.
func NewTextReportBuilder(outputDir string) reporter.ReportBuilder {
	return &TextReportBuilder{outputDir: outputDir}
}

// ReportType returns the type of report this builder generates.
func (b *TextReportBuilder) ReportType() string {
	return "TextSummary"
}

type summaryFileWriter struct {
	f *os.File
}

func (sfw *summaryFileWriter) writeLine(format string, args ...interface{}) {
	fmt.Fprintf(sfw.f, format+"\n", args...)
}

// CreateReport generates the text summary report using the analyzed model.SummaryResult.
func (b *TextReportBuilder) CreateReport(summary *model.SummaryResult) error {
	if err := os.MkdirAll(b.outputDir, 0755); err != nil {
		return fmt.Errorf("failed to create target directory: %w", err)
	}

	outputPath := filepath.Join(b.outputDir, "Summary.txt")
	f, err := os.Create(outputPath)
	if err != nil {
		return fmt.Errorf("failed to create report file: %w", err)
	}
	defer f.Close()

	sfw := &summaryFileWriter{f: f}

	sfw.writeLine("Summary")
	sfw.writeLine("  Generated on: %s", time.Now().Format("02/01/2006 - 15:04:05"))
	
	if summary.Timestamp > 0 {
		sfw.writeLine("  Coverage date: %s", time.Unix(summary.Timestamp, 0).Format("02/01/2006 - 15:04:05"))
	}
	
	sfw.writeLine("  Parser: %s", summary.ParserName)

	totalClasses := 0
	totalFiles := 0
	processedFilePaths := make(map[string]bool)
	for _, assembly := range summary.Assemblies {
		totalClasses += len(assembly.Classes)
		for _, class := range assembly.Classes {
			for _, codeFile := range class.Files {
				if !processedFilePaths[codeFile.Path] {
					processedFilePaths[codeFile.Path] = true
					totalFiles++
				}
			}
		}
	}

	sfw.writeLine("  Assemblies: %d", len(summary.Assemblies))
	sfw.writeLine("  Classes: %d", totalClasses)
	sfw.writeLine("  Files: %d", totalFiles)

	overallLineRate := 0.0
	if summary.LinesValid > 0 {
		overallLineRate = (float64(summary.LinesCovered) / float64(summary.LinesValid)) * 100
	}
	sfw.writeLine("  Line coverage: %.1f%%", overallLineRate)
	sfw.writeLine("  Covered lines: %d", summary.LinesCovered)
	sfw.writeLine("  Uncovered lines: %d", summary.LinesValid-summary.LinesCovered)
	sfw.writeLine("  Coverable lines: %d", summary.LinesValid)
	if summary.TotalLines > 0 {
		sfw.writeLine("  Total lines: %d", summary.TotalLines)
	} else {
		sfw.writeLine("  Total lines: N/A")
	}

	// Conditional Branch Coverage Output
	if summary.BranchesValid != nil && summary.BranchesCovered != nil { // Check if pointers are set
		if *summary.BranchesValid > 0 { // Only print percentage if there are valid branches
			overallBranchRate := (float64(*summary.BranchesCovered) / float64(*summary.BranchesValid)) * 100
			sfw.writeLine("  Branch coverage: %.1f%% (%d of %d)", overallBranchRate, *summary.BranchesCovered, *summary.BranchesValid)
		}
		// Always print these if the data was present in the report, even if 0
		sfw.writeLine("  Covered branches: %d", *summary.BranchesCovered)
		sfw.writeLine("  Total branches: %d", *summary.BranchesValid)
	}


	totalMethods, coveredMethods, fullyCoveredMethods := 0, 0, 0
	for _, assembly := range summary.Assemblies {
		for _, class := range assembly.Classes {
			for _, method := range class.Methods {
				totalMethods++
				methodLinesValid := 0
				methodLinesCovered := 0
				if method.Lines != nil {
					for _, line := range method.Lines {
						methodLinesValid++
						if line.Hits > 0 {
							methodLinesCovered++
						}
					}
				}
				if methodLinesCovered > 0 {
					coveredMethods++
				}
				if methodLinesValid > 0 && methodLinesCovered == methodLinesValid {
					fullyCoveredMethods++
				}
			}
		}
	}
	methodCoverage := 0.0
	if totalMethods > 0 {
		methodCoverage = (float64(coveredMethods) / float64(totalMethods)) * 100
	}
	fullMethodCoverage := 0.0
	if totalMethods > 0 {
		fullMethodCoverage = (float64(fullyCoveredMethods) / float64(totalMethods)) * 100
	}
	sfw.writeLine("  Method coverage: %.1f%% (%d of %d)", methodCoverage, coveredMethods, totalMethods)
	sfw.writeLine("  Full method coverage: %.1f%% (%d of %d)", fullMethodCoverage, fullyCoveredMethods, totalMethods)
	sfw.writeLine("  Covered methods: %d", coveredMethods)
	sfw.writeLine("  Fully covered methods: %d", fullyCoveredMethods)
	sfw.writeLine("  Total methods: %d", totalMethods)

	tw := tabwriter.NewWriter(f, 0, 0, 2, ' ', 0)
	defer tw.Flush()
	for _, assembly := range summary.Assemblies {
		fmt.Fprintln(tw)
		assemblyLineRate := 0.0
		if assembly.LinesValid > 0 {
			assemblyLineRate = (float64(assembly.LinesCovered) / float64(assembly.LinesValid)) * 100
		}
		fmt.Fprintf(tw, "%s\t  %.1f%%\n", assembly.Name, assemblyLineRate)
		sortedClasses := make([]model.Class, len(assembly.Classes))
		copy(sortedClasses, assembly.Classes)
		sort.Slice(sortedClasses, func(i, j int) bool {
			return sortedClasses[i].DisplayName < sortedClasses[j].DisplayName
		})
		for _, class := range sortedClasses {
			classLineRate := 0.0
			if class.LinesValid > 0 {
				classLineRate = (float64(class.LinesCovered) / float64(class.LinesValid)) * 100
			}
			fmt.Fprintf(tw, "  %s\t  %.1f%%\n", class.DisplayName, classLineRate)
		}
	}
	return nil
}
