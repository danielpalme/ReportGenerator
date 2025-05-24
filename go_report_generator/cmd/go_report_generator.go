package main

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/parser"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/reporter"
)

// supportedReportTypes defines the available report formats
var supportedReportTypes = map[string]bool{
	"TextSummary": true,
	"Html":        true,
}

// validateReportTypes checks if all requested report types are supported
func validateReportTypes(types []string) error {
	for _, t := range types {
		if !supportedReportTypes[t] {
			return fmt.Errorf("unsupported report type: %s", t)
		}
	}
	return nil
}

func main() {	start := time.Now()

	// Parse command line arguments
	reportPath := flag.String("report", "", "Path to Cobertura XML file")
	outputDir := flag.String("output", "coverage-report", "Output directory for reports")
	reportTypes := flag.String("reporttypes", "TextSummary", "Report types to generate (comma-separated: TextSummary,Html)")
	flag.Parse()

	// Validate required arguments
	if *reportPath == "" {
		fmt.Println("Usage: go_report_generator -report <cobertura.xml> [-output <dir>] [-reporttypes <types>]")
		fmt.Println("\nReport types:")
		fmt.Println("  TextSummary  Generate a text summary report")
		fmt.Println("  Html         Generate an HTML coverage report")
		os.Exit(1)
	}

	// Validate report types 
	requestedTypes := strings.Split(*reportTypes, ",")
	if err := validateReportTypes(requestedTypes); err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		fmt.Println("\nSupported report types: TextSummary, Html")
		os.Exit(1)
	}

	fmt.Printf("Parsing coverage report: %s\n", *reportPath)

	// Parse the coverage report
	report, err := parser.ParseCobertura(*reportPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to parse coverage report: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("Coverage report parsed successfully in %.2f seconds\n", time.Since(start).Seconds())
	fmt.Printf("Generating reports in: %s\n", *outputDir)

	// Create output directory if it doesn't exist
	if err := os.MkdirAll(*outputDir, 0755); err != nil {
		fmt.Fprintf(os.Stderr, "Failed to create output directory: %v\n", err)
		os.Exit(1)	}

	// Generate each requested report type
	for _, reportType := range requestedTypes {
		fmt.Printf("Generating %s report...\n", reportType)

		switch reportType {
		case "TextSummary":
			textBuilder := reporter.NewTextReportBuilder(*outputDir)
			if err := textBuilder.CreateReport(report); err != nil {
				fmt.Fprintf(os.Stderr, "Failed to generate text report: %v\n", err)
				os.Exit(1)
			}			
			fmt.Printf("Text report generated at: %s\n", filepath.Join(*outputDir, "Summary.txt"))
		case "Html":
			fmt.Printf("HTML report generation is a placeholder for now (coming soon)\n")
		}
	}

	fmt.Printf("\nReport generation completed in %.2f seconds\n", time.Since(start).Seconds())
}
