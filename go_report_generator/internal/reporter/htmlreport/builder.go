package htmlreport

import (
	"fmt"
	"html/template"
	"io"
	"os"
	"path/filepath"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

const assetsDir = "../assets/htmlreport/" // Adjusted path relative to cmd dir

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

	// Copy static assets
	if err := b.copyStaticAssets(); err != nil {
		return fmt.Errorf("failed to copy static assets: %w", err)
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

// copyStaticAssets copies static assets to the output directory.
func (b *HtmlReportBuilder) copyStaticAssets() error {
	filesToCopy := []string{
		"custom.css",
		"custom.js",
		"chartist.min.css",
		"chartist.min.js",
		"custom-azurepipelines.css",
		"custom-azurepipelines_adaptive.css",
		"custom-azurepipelines_dark.css",
		"custom_adaptive.css",
		"custom_bluered.css",
		"custom_dark.css",
	}

	for _, fileName := range filesToCopy {
		srcPath := filepath.Join(assetsDir, fileName)
		dstPath := filepath.Join(b.OutputDir, fileName)

		srcFile, err := os.Open(srcPath)
		if err != nil {
			return fmt.Errorf("failed to open source asset %s: %w", srcPath, err)
		}
		defer srcFile.Close()

		dstFile, err := os.Create(dstPath)
		if err != nil {
			return fmt.Errorf("failed to create destination asset %s: %w", dstPath, err)
		}
		defer dstFile.Close()

		if _, err := io.Copy(dstFile, srcFile); err != nil {
			return fmt.Errorf("failed to copy asset from %s to %s: %w", srcPath, dstPath, err)
		}
	}

	// Combine custom.css and custom_dark.css into report.css
	customCSSPath := filepath.Join(assetsDir, "custom.css")
	customDarkCSSPath := filepath.Join(assetsDir, "custom_dark.css")

	customCSSBytes, err := os.ReadFile(customCSSPath)
	if err != nil {
		return fmt.Errorf("failed to read custom.css from %s: %w", customCSSPath, err)
	}

	customDarkCSSBytes, err := os.ReadFile(customDarkCSSPath)
	if err != nil {
		return fmt.Errorf("failed to read custom_dark.css from %s: %w", customDarkCSSPath, err)
	}

	var combinedCSS []byte
	combinedCSS = append(combinedCSS, customCSSBytes...)
	combinedCSS = append(combinedCSS, []byte("\n")...) // Add a newline separator
	combinedCSS = append(combinedCSS, customDarkCSSBytes...)

	reportCSSPath := filepath.Join(b.OutputDir, "report.css")
	if err := os.WriteFile(reportCSSPath, combinedCSS, 0644); err != nil {
		return fmt.Errorf("failed to write combined report.css to %s: %w", reportCSSPath, err)
	}

	return nil
}
