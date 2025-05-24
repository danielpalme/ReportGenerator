package reporter

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"text/tabwriter" // Import for tabwriter
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

type TextReportBuilder struct {
	targetDir string
}

func NewTextReportBuilder(targetDir string) *TextReportBuilder {
	return &TextReportBuilder{targetDir: targetDir}
}

func (b *TextReportBuilder) ReportType() string {
	return "TextSummary"
}

type AggregatedClassData struct {
	ParentPackageName string
	OriginalKey       string
	DisplayName       string
	LinesCovered      int
	LinesValid        int
	LineRate          float64 // Stored as 0-100 value
}

func getCleanAggregationKey(rawClassName string, packageName string) string {
	name := rawClassName
	if idx := strings.Index(name, "/<"); idx != -1 {
		name = name[:idx]
	} else if idx := strings.Index(name, "+<"); idx != -1 {
		name = name[:idx]
	}
	if idx := strings.Index(name, "/"); idx != -1 {
		name = name[:idx]
	}
	return name
}

func formatDisplayName(aggregationKey string) string {
	name := aggregationKey
	if strings.Contains(name, "`3") {
		name = strings.ReplaceAll(name, "`3", "<T1, T2, T3>")
	}
	if strings.Contains(name, "`2") {
		name = strings.ReplaceAll(name, "`2", "<T1, T2>")
	}
	if strings.Contains(name, "`1") {
		name = strings.ReplaceAll(name, "`1", "<T>")
	}
	return name
}

type summaryWriter struct {
	f *os.File
}

func (sw *summaryWriter) writeLine(format string, args ...interface{}) {
	fmt.Fprintf(sw.f, format+"\n", args...)
}


func (b *TextReportBuilder) CreateReport(report *model.CoverageReport) error {
	if err := os.MkdirAll(b.targetDir, 0755); err != nil {
		return fmt.Errorf("failed to create target directory: %w", err)
	}

	outputPath := filepath.Join(b.targetDir, "Summary.txt")
	f, err := os.Create(outputPath)
	if err != nil {
		return fmt.Errorf("failed to create report file: %w", err)
	}
	defer f.Close()

	sw := &summaryWriter{f: f}

	sw.writeLine("Summary")
	sw.writeLine("  Generated on: %s", time.Now().Format("02/01/2006 - 15:04:05"))
	if report.Timestamp > 0 {
		sw.writeLine("  Coverage date: %s", time.Unix(report.Timestamp, 0).Format("02/01/2006 - 15:04:05"))
	}
	sw.writeLine("  Parser: Cobertura")

	uniqueAggregatedClasses := make(map[string]*AggregatedClassData)
	totalFiles := make(map[string]bool)

	for _, pkg := range report.Packages {
		for _, class := range pkg.Classes {
			totalFiles[class.Filename] = true
			aggKey := getCleanAggregationKey(class.Name, pkg.Name)
			data, exists := uniqueAggregatedClasses[aggKey]
			if !exists {
				data = &AggregatedClassData{OriginalKey: aggKey, ParentPackageName: pkg.Name}
				uniqueAggregatedClasses[aggKey] = data
			}
			for _, line := range class.Lines {
				data.LinesValid++
				if line.Hits > 0 {
					data.LinesCovered++
				}
			}
		}
	}
    
    // Calculate LineRate for aggregated classes (as 0-100)
	// Also, pre-calculate package line rates based on aggregated classes for accuracy.
	packageLineRates := make(map[string]float64)
	packageLinesCovered := make(map[string]int)
	packageLinesValid := make(map[string]int)

	for key, data := range uniqueAggregatedClasses {
		if data.LinesValid > 0 {
			data.LineRate = (float64(data.LinesCovered) / float64(data.LinesValid)) * 100
		} else {
            data.LineRate = 0.0 // Or 100.0 if no lines means fully covered (e.g. interfaces)
        }
		data.DisplayName = formatDisplayName(key)

		packageLinesCovered[data.ParentPackageName] += data.LinesCovered
		packageLinesValid[data.ParentPackageName] += data.LinesValid
	}

	for _, pkg := range report.Packages {
		if validLines, ok := packageLinesValid[pkg.Name]; ok && validLines > 0 {
			packageLineRates[pkg.Name] = (float64(packageLinesCovered[pkg.Name]) / float64(validLines)) * 100
		} else {
            // If a package has no coverable lines in any of its aggregated classes,
            // its rate could be 0% or 100% based on convention (e.g. package of interfaces)
            // For consistency with class logic, let's use 0% if no valid lines.
            // C# ReportGenerator might use report.Package.CoverageQuota directly which might be pre-calculated.
            // Using report.LineRate from input model for package if no classes found, else calculated.
            if len(uniqueAggregatedClasses) == 0 && len(report.Packages) ==1 { // Or a better check
                 packageLineRates[pkg.Name] = report.LineRate * 100 // Assuming report.LineRate is for the single package
            } else {
                 packageLineRates[pkg.Name] = 0.0
            }
        }
	}


	sw.writeLine("  Assemblies: %d", len(report.Packages))
	sw.writeLine("  Classes: %d", len(uniqueAggregatedClasses))
	sw.writeLine("  Files: %d", len(totalFiles))
	sw.writeLine("  Line coverage: %.1f%%", report.LineRate*100) // report.LineRate is 0.0-1.0
	sw.writeLine("  Covered lines: %d", report.LinesCovered)
	sw.writeLine("  Uncovered lines: %d", report.LinesValid-report.LinesCovered)
	sw.writeLine("  Coverable lines: %d", report.LinesValid)
	// ** TOTAL LINES **
	// This requires data from your model. If `report.TotalPhysicalLines` (or similar) exists:
	// sw.writeLine("  Total lines: %d", report.TotalPhysicalLines)
	// Otherwise, indicate it's not available or omit:
	sw.writeLine("  Total lines: N/A")


	if report.BranchesValid > 0 {
		sw.writeLine("  Branch coverage: %.1f%% (%d of %d)", report.BranchRate*100, report.BranchesCovered, report.BranchesValid) // report.BranchRate is 0.0-1.0
		sw.writeLine("  Covered branches: %d", report.BranchesCovered)
		sw.writeLine("  Total branches: %d", report.BranchesValid)
	}

	totalMethods, coveredMethods, fullyCoveredMethods := 0, 0, 0
	for _, pkg := range report.Packages {
		for _, class := range pkg.Classes {
			for _, method := range class.Methods {
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
	// These are already 0-100 values
	sw.writeLine("  Method coverage: %.1f%% (%d of %d)", methodCoverage, coveredMethods, totalMethods)
	sw.writeLine("  Full method coverage: %.1f%% (%d of %d)", fullMethodCoverage, fullyCoveredMethods, totalMethods)
	sw.writeLine("  Covered methods: %d", coveredMethods)
	sw.writeLine("  Fully covered methods: %d", fullyCoveredMethods)
	sw.writeLine("  Total methods: %d", totalMethods)

	// Initialize tabwriter for the package/class list
	// Using padding of 2 to ensure enough space, and "  " before percentage for visual match
	tw := tabwriter.NewWriter(f, 0, 0, 2, ' ', 0) 
	defer tw.Flush()

	for _, pkg := range report.Packages {
		fmt.Fprintln(tw) 
        pkgRate := packageLineRates[pkg.Name] // Use calculated package rate
		fmt.Fprintf(tw, "%s\t  %.1f%%\n", pkg.Name, pkgRate) // pkgRate is already 0-100

		var classesInPackage []*AggregatedClassData
		for _, aggData := range uniqueAggregatedClasses {
			if aggData.ParentPackageName == pkg.Name || strings.HasPrefix(aggData.OriginalKey, pkg.Name+".") {
				classesInPackage = append(classesInPackage, aggData)
			}
		}
		sort.Slice(classesInPackage, func(i, j int) bool {
			return classesInPackage[i].DisplayName < classesInPackage[j].DisplayName
		})
		for _, aggData := range classesInPackage {
            // aggData.LineRate is already 0-100
			fmt.Fprintf(tw, "  %s\t  %.1f%%\n", aggData.DisplayName, aggData.LineRate) 
		}
	}
	return nil
}