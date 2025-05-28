package htmlreport

import (
	"fmt"
	"html/template"
	"os"
	"path/filepath"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// HtmlReportBuilder is responsible for generating HTML reports.
type HtmlReportBuilder struct {
	OutputDir string
}

// NewHtmlReportBuilder creates a new HtmlReportBuilder.
func NewHtmlReportBuilder(outputDir string) *HtmlReportBuilder {
	return &HtmlReportBuilder{
		OutputDir: outputDir,
	}
}

// ReportType returns the type of report this builder creates.
func (b *HtmlReportBuilder) ReportType() string {
	return "Html"
}

// CreateReport generates the HTML report based on the SummaryResult.
// For now, it's a placeholder and will return nil.
func (b *HtmlReportBuilder) CreateReport(report *model.SummaryResult) error {
	// Ensure output directory exists
	if err := os.MkdirAll(b.OutputDir, 0755); err != nil {
		return fmt.Errorf("failed to create output directory %s: %w", b.OutputDir, err)
	}

	// Prepare data for the template
	var generatedAtStr string
	if report.Timestamp == 0 {
		generatedAtStr = "N/A"
	} else {
		generatedAtStr = time.Unix(report.Timestamp, 0).Format(time.RFC1123Z)
	}

	data := struct {
		Title       string
		ParserName  string
		GeneratedAt string
		Content     template.HTML
	}{
		Title:       "Coverage Report",
		ParserName:  report.ParserName,
		GeneratedAt: generatedAtStr,
		Content:     template.HTML("<p>Main content goes here!</p>"), // Placeholder content
	}

	// Create index.html file
	indexPath := filepath.Join(b.OutputDir, "index.html")
	file, err := os.Create(indexPath)
	if err != nil {
		return fmt.Errorf("failed to create index.html at %s: %w", indexPath, err)
	}
	defer file.Close()

	// Execute the base template with the data
	// baseTpl is parsed in templates.go and available in this package
	if err := baseTpl.Execute(file, data); err != nil {
		return fmt.Errorf("failed to execute base template into %s: %w", indexPath, err)
	}

	return nil
}
