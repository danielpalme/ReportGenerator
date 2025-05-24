package parser

import (
	"os"
	"path/filepath"
	"testing"
)

// Helper: load test fixture XML file
func loadFixture(t *testing.T, name string) string {
	t.Helper()
	path := filepath.Join("testdata", name)
	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("failed to read fixture %s: %v", name, err)
	}
	return string(data)
}

func TestParseCobertura_ValidReport(t *testing.T) {
	reportPath := filepath.Join("testdata", "cobertura_sample.xml")
	result, err := ParseCobertura(reportPath)
	if err != nil {
		t.Fatalf("ParseCobertura failed: %v", err)
	}
	// Assert on model.CoverageReport fields
	if result.LineRate <= 0.0 {
		t.Errorf("Expected LineRate > 0, got %v", result.LineRate)
	}
	if len(result.Packages) == 0 {
		t.Errorf("Expected at least 1 package")
	}
	// Add more checks as needed (classes, methods, lines, etc.)
}

func TestParseCobertura_InvalidXML(t *testing.T) {
	invalidPath := filepath.Join("testdata", "invalid.xml")
	_, err := ParseCobertura(invalidPath)
	if err == nil {
		t.Errorf("Expected error for invalid XML, got nil")
	}
}