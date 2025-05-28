package main

import (
	"flag"
	"fmt"
	"os"
	"strings"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/analyzer"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/parser"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/reporter/htmlreport" // Added import
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/reporter/textsummary"
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

func main() {
	start := time.Now()

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

	fmt.Printf("Processing coverage report: %s\n", *reportPath)

	// Step 1: Parse the Cobertura XML into raw input structures
	rawReport, sourceDirs, err := parser.ParseCoberturaXML(*reportPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to parse Cobertura XML: %v\n", err)
		os.Exit(1)
	}
	fmt.Printf("Cobertura XML parsed successfully.\n")

	// Step 2: Analyze the raw report to produce the enriched model.SummaryResult
	// Note: The current analyzer is a basic placeholder.
	summaryResult, err := analyzer.Analyze(rawReport, sourceDirs)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to analyze coverage data: %v\n", err)
		os.Exit(1)
	}
	fmt.Printf("Coverage data analyzed (using placeholder analyzer).\n")

	fmt.Printf("Generating reports in: %s\n", *outputDir)

	// Create output directory if it doesn't exist
	if err := os.MkdirAll(*outputDir, 0755); err != nil {
		fmt.Fprintf(os.Stderr, "Failed to create output directory: %v\n", err)
		os.Exit(1)
	}

	// Generate each requested report type
	for _, reportType := range requestedTypes {
		fmt.Printf("Generating %s report...\n", reportType)

		// Step 3: Generate reports using the summaryResult
		switch reportType {
		case "TextSummary":
			// Use the new textsummary reporter
			textBuilder := textsummary.NewTextReportBuilder(*outputDir)
			if err := textBuilder.CreateReport(summaryResult); err != nil {
				fmt.Fprintf(os.Stderr, "Failed to generate text report: %v\n", err)
				os.Exit(1) // Keep exit for text summary as it might be critical for CI
			}
		case "Html":
			// Instantiate HtmlReportBuilder and create the report
			htmlBuilder := htmlreport.NewHtmlReportBuilder(*outputDir)
			if err := htmlBuilder.CreateReport(summaryResult); err != nil {
				fmt.Fprintf(os.Stderr, "Failed to generate HTML report: %v\n", err)
				// Do not os.Exit(1) here to allow other reports to be generated
			}
		}
	}

	fmt.Printf("\nReport generation completed in %.2f seconds\n", time.Since(start).Seconds())
}
