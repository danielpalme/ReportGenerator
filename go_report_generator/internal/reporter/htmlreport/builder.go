package htmlreport

import (
	"fmt"
	"html/template"
	"io"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/utils"
	"golang.org/x/net/html"
)

var (
	// Absolute paths to asset directories
	assetsDir            = filepath.Join(utils.ProjectRoot(), "assets", "htmlreport")
	angularDistSourcePath = filepath.Join(utils.ProjectRoot(), "angular_frontend_spa", "dist")
)

// HTMLReportData holds all data for the base HTML template.
type HTMLReportData struct {
	Title                  string
	ParserName             string
	GeneratedAt            string
	Content                template.HTML
	AngularCssFile         string
	AngularRuntimeJsFile   string
	AngularPolyfillsJsFile string
	AngularMainJsFile      string
}

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

	// Copy Angular assets
	if err := b.copyAngularAssets(b.OutputDir); err != nil {
		return fmt.Errorf("failed to copy angular assets: %w", err)
	}

	// Parse Angular index.html to get asset filenames
	angularIndexHTMLPath := filepath.Join(angularDistSourcePath, "index.html")
	cssFile, runtimeJs, polyfillsJs, mainJs, err := b.parseAngularIndexHTML(angularIndexHTMLPath)
	if err != nil {
		return fmt.Errorf("failed to parse Angular index.html: %w", err)
	}
	if cssFile == "" || runtimeJs == "" || polyfillsJs == "" || mainJs == "" {
		return fmt.Errorf("missing one or more critical Angular assets from index.html (css: %s, runtime: %s, polyfills: %s, main: %s)", cssFile, runtimeJs, polyfillsJs, mainJs)
	}


	// Prepare data for the template
	var generatedAtStr string
	if report.Timestamp == 0 {
		generatedAtStr = "N/A"
	} else {
		generatedAtStr = time.Unix(report.Timestamp, 0).Format(time.RFC1123Z)
	}

	data := HTMLReportData{
		Title:                  "Coverage Report",
		ParserName:             report.ParserName,
		GeneratedAt:            generatedAtStr,
		Content:                template.HTML("<p>Main content will be replaced by Angular app.</p>"), // Placeholder, Angular will take over
		AngularCssFile:         cssFile,
		AngularRuntimeJsFile:   runtimeJs,
		AngularPolyfillsJsFile: polyfillsJs,
		AngularMainJsFile:      mainJs,
	}

	// Create index.html file in the output directory
	outputIndexPath := filepath.Join(b.OutputDir, "index.html")
	file, err := os.Create(outputIndexPath)
	if err != nil {
		return fmt.Errorf("failed to create index.html at %s: %w", outputIndexPath, err)
	}
	defer file.Close()

	// Execute the base template with the data
	// baseTpl is parsed in templates.go and available in this package
	if err := baseTpl.Execute(file, data); err != nil {
		return fmt.Errorf("failed to execute base template into %s: %w", outputIndexPath, err)
	}

	return nil
}

// parseAngularIndexHTML parses the Angular generated index.html to find asset filenames.
func (b *HtmlReportBuilder) parseAngularIndexHTML(angularIndexHTMLPath string) (cssFile, runtimeJs, polyfillsJs, mainJs string, err error) {
	file, err := os.Open(angularIndexHTMLPath)
	if err != nil {
		return "", "", "", "", fmt.Errorf("failed to open Angular index.html at %s: %w", angularIndexHTMLPath, err)
	}
	defer file.Close()

	doc, err := html.Parse(file)
	if err != nil {
		return "", "", "", "", fmt.Errorf("failed to parse Angular index.html: %w", err)
	}

	var f func(*html.Node)
	f = func(n *html.Node) {
		if n.Type == html.ElementNode {
			if n.Data == "link" {
				isStylesheet := false
				var href string
				for _, a := range n.Attr {
					if a.Key == "rel" && a.Val == "stylesheet" {
						isStylesheet = true
					}
					if a.Key == "href" {
						href = a.Val
					}
				}
				if isStylesheet && href != "" {
					cssFile = href // Expects only one main stylesheet
				}
			} else if n.Data == "script" {
				var src string
				for _, a := range n.Attr {
					if a.Key == "src" {
						src = a.Val
						break
					}
				}
				if src != "" {
					if strings.HasPrefix(filepath.Base(src), "runtime.") && strings.HasSuffix(src, ".js") {
						runtimeJs = src
					} else if strings.HasPrefix(filepath.Base(src), "polyfills.") && strings.HasSuffix(src, ".js") {
						polyfillsJs = src
					} else if strings.HasPrefix(filepath.Base(src), "main.") && strings.HasSuffix(src, ".js") {
						mainJs = src
					}
				}
			}
		}
		for c := n.FirstChild; c != nil; c = c.NextSibling {
			f(c)
		}
	}
	f(doc)

	// Basic validation: check if any of the crucial files were not found
	if cssFile == "" {
		err = fmt.Errorf("could not find Angular CSS file in %s", angularIndexHTMLPath)
	} else if runtimeJs == "" {
		err = fmt.Errorf("could not find Angular runtime.js file in %s", angularIndexHTMLPath)
	} else if polyfillsJs == "" {
		err = fmt.Errorf("could not find Angular polyfills.js file in %s", angularIndexHTMLPath)
	} else if mainJs == "" {
		err = fmt.Errorf("could not find Angular main.js file in %s", angularIndexHTMLPath)
	}

	return cssFile, runtimeJs, polyfillsJs, mainJs, err
}

// copyAngularAssets copies the built Angular application assets to the output directory.
func (b *HtmlReportBuilder) copyAngularAssets(outputDir string) error {
	// Ensure the source directory exists
	srcInfo, err := os.Stat(angularDistSourcePath)
	if err != nil {
		if os.IsNotExist(err) {
			return fmt.Errorf("angular source directory %s does not exist: %w", angularDistSourcePath, err)
		}
		return fmt.Errorf("failed to stat angular source directory %s: %w", angularDistSourcePath, err)
	}
	if !srcInfo.IsDir() {
		return fmt.Errorf("angular source path %s is not a directory", angularDistSourcePath)
	}

	// Walk the Angular distribution directory
	return filepath.WalkDir(angularDistSourcePath, func(srcPath string, d fs.DirEntry, err error) error {
		if err != nil {
			return fmt.Errorf("error accessing path %s during walk: %w", srcPath, err)
		}

		// Determine the relative path from the source root
		relPath, err := filepath.Rel(angularDistSourcePath, srcPath)
		if err != nil {
			return fmt.Errorf("failed to get relative path for %s: %w", srcPath, err)
		}

		// Construct the destination path
		dstPath := filepath.Join(outputDir, relPath)

		if d.IsDir() {
			// Create the directory in the destination
			if err := os.MkdirAll(dstPath, d.Type().Perm()); err != nil { // Use source permission
				return fmt.Errorf("failed to create directory %s: %w", dstPath, err)
			}
		} else {
			// It's a file, copy it
			srcFile, err := os.Open(srcPath)
			if err != nil {
				return fmt.Errorf("failed to open source file %s: %w", srcPath, err)
			}
			defer srcFile.Close()

			// Ensure the destination directory exists (it should, if WalkDir processes dirs first, but good to be safe)
			if err := os.MkdirAll(filepath.Dir(dstPath), 0755); err != nil {
				return fmt.Errorf("failed to create parent directory for %s: %w", dstPath, err)
			}

			dstFile, err := os.Create(dstPath)
			if err != nil {
				return fmt.Errorf("failed to create destination file %s: %w", dstPath, err)
			}
			defer dstFile.Close()

			if _, err := io.Copy(dstFile, srcFile); err != nil {
				return fmt.Errorf("failed to copy file from %s to %s: %w", srcPath, dstPath, err)
			}

			// Attempt to set file permissions to match source - best effort
			srcFileInfo, statErr := d.Info()
			if statErr == nil {
				os.Chmod(dstPath, srcFileInfo.Mode())
			}
		}
		return nil
	})
}

// copyStaticAssets copies static assets to the output directory.
// Note: outputDir argument was removed as b.OutputDir is used directly.
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
		// Construct source path relative to the assetsDir constant
		srcPath := filepath.Join(assetsDir, fileName)
		// Construct destination path relative to the builder's OutputDir
		dstPath := filepath.Join(b.OutputDir, fileName)

		// Ensure destination directory for the asset exists
		dstDir := filepath.Dir(dstPath)
		if err := os.MkdirAll(dstDir, 0755); err != nil {
			return fmt.Errorf("failed to create directory for asset %s: %w", dstPath, err)
		}

		srcFile, err := os.Open(srcPath)
		if err != nil {
			// Try to give a more specific error if the source asset itself is not found
			if os.IsNotExist(err) {
				return fmt.Errorf("source asset %s not found: %w", srcPath, err)
			}
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
	// Source paths are relative to assetsDir
	customCSSPath := filepath.Join(assetsDir, "custom.css")
	customDarkCSSPath := filepath.Join(assetsDir, "custom_dark.css")

	customCSSBytes, err := os.ReadFile(customCSSPath)
	if err != nil {
		if os.IsNotExist(err) {
			return fmt.Errorf("custom.css not found at %s: %w", customCSSPath, err)
		}
		return fmt.Errorf("failed to read custom.css from %s: %w", customCSSPath, err)
	}

	customDarkCSSBytes, err := os.ReadFile(customDarkCSSPath)
	if err != nil {
		if os.IsNotExist(err) {
			return fmt.Errorf("custom_dark.css not found at %s: %w", customDarkCSSPath, err)
		}
		return fmt.Errorf("failed to read custom_dark.css from %s: %w", customDarkCSSPath, err)
	}

	var combinedCSS []byte
	combinedCSS = append(combinedCSS, customCSSBytes...)
	combinedCSS = append(combinedCSS, []byte("\n")...) // Add a newline separator
	combinedCSS = append(combinedCSS, customDarkCSSBytes...)

	// Destination path for report.css is relative to builder's OutputDir
	reportCSSPath := filepath.Join(b.OutputDir, "report.css")
	if err := os.WriteFile(reportCSSPath, combinedCSS, 0644); err != nil {
		return fmt.Errorf("failed to write combined report.css to %s: %w", reportCSSPath, err)
	}

	return nil
}
