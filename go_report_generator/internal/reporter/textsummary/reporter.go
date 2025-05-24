// In: internal/reporter/textsummary/reporter.go
package textsummary

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"

	// "strings" // May not be needed if DisplayName is pre-formatted
	"text/tabwriter"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"    // This is your new model package
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/reporter" // For the ReportBuilder interface
)

// TextReportBuilder generates a text summary report.
type TextReportBuilder struct {
	outputDir string
}

// NewTextReportBuilder creates a new TextReportBuilder.
func NewTextReportBuilder(outputDir string) reporter.ReportBuilder { // Ensure this matches your interface definition path
	return &TextReportBuilder{outputDir: outputDir}
}

// ReportType returns the type of report this builder generates.
func (b *TextReportBuilder) ReportType() string {
	return "TextSummary"
}

type summaryFileWriter struct { // Renamed from summaryWriter to avoid conflict if used elsewhere
	f *os.File
}

func (sfw *summaryFileWriter) writeLine(format string, args ...interface{}) {
	fmt.Fprintf(sfw.f, format+"\n", args...)
}

// CreateReport generates the text summary report using the analyzed model.SummaryResult.
func (b *TextReportBuilder) CreateReport(summary *model.SummaryResult) error { // Takes *model.SummaryResult
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
	if summary.Timestamp > 0 { // Use summary.Timestamp
		sfw.writeLine("  Coverage date: %s", time.Unix(summary.Timestamp, 0).Format("02/01/2006 - 15:04:05"))
	}
	sfw.writeLine("  Parser: %s", summary.ParserName) // Use summary.ParserName

	// --- Data from summary (already aggregated by analyzer) ---
	totalClasses := 0
	totalFiles := 0 // This needs to be calculated by analyzer if needed, or re-calculated here
	                   // C# SummaryResult.TotalLines logic sums unique files.
	                   // For now, let's count unique files from the analyzed model if not directly in summary.
	
	processedFilePaths := make(map[string]bool)
	for _, assembly := range summary.Assemblies {
		totalClasses += len(assembly.Classes)
		for _, class := range assembly.Classes {
			for _, codeFile := range class.Files { // Assuming class.Files is populated
				if !processedFilePaths[codeFile.Path] {
					processedFilePaths[codeFile.Path] = true
					totalFiles++
				}
			}
		}
	}
    // --- End data from summary ---


	sfw.writeLine("  Assemblies: %d", len(summary.Assemblies))
	sfw.writeLine("  Classes: %d", totalClasses)  // Use count from iteration
	sfw.writeLine("  Files: %d", totalFiles)      // Use count from iteration

    // Overall Line Coverage - assuming summary fields are direct sums from analyzer
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

    overallBranchRate := 0.0
	if summary.BranchesValid > 0 {
        if summary.BranchesValid > 0 {
            overallBranchRate = (float64(summary.BranchesCovered) / float64(summary.BranchesValid)) * 100
        }
		sfw.writeLine("  Branch coverage: %.1f%% (%d of %d)", overallBranchRate, summary.BranchesCovered, summary.BranchesValid)
		sfw.writeLine("  Covered branches: %d", summary.BranchesCovered)
		sfw.writeLine("  Total branches: %d", summary.BranchesValid)
	}

	// Method metrics - still calculated here from the model, but could also move to analyzer
	totalMethods, coveredMethods, fullyCoveredMethods := 0, 0, 0
	for _, assembly := range summary.Assemblies {
		for _, class := range assembly.Classes {
			for _, method := range class.Methods { // Assuming model.Class has model.Method
				totalMethods++
				methodLinesValid, methodLinesCovered := 0, 0
				for _, line := range method.Lines {
					methodLinesValid++
					if line.Hits > 0 {
						methodLinesCovered++
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

	for _, assembly := range summary.Assemblies { // Iterate over model.Assembly
		fmt.Fprintln(tw)
        
        assemblyLineRate := 0.0
        if assembly.LinesValid > 0 {
            assemblyLineRate = (float64(assembly.LinesCovered) / float64(assembly.LinesValid)) * 100
        }
		fmt.Fprintf(tw, "%s\t  %.1f%%\n", assembly.Name, assemblyLineRate)

		// Sort classes by DisplayName for consistent output
		// Create a temporary slice to sort, as summary.Assemblies[i].Classes might be a direct slice from map iteration
		sortedClasses := make([]model.Class, len(assembly.Classes))
		copy(sortedClasses, assembly.Classes)
		sort.Slice(sortedClasses, func(i, j int) bool {
			return sortedClasses[i].DisplayName < sortedClasses[j].DisplayName
		})

		for _, class := range sortedClasses { // Iterate over model.Class
            classLineRate := 0.0
            if class.LinesValid > 0 {
                classLineRate = (float64(class.LinesCovered) / float64(class.LinesValid)) * 100
            }
			// class.DisplayName is assumed to be pre-formatted by the analyzer
			fmt.Fprintf(tw, "  %s\t  %.1f%%\n", class.DisplayName, classLineRate)
		}
	}
	return nil
}