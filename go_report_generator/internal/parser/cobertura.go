package parser

import (
	"encoding/xml"
	"io"
	"os"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
)

// ParseCoberturaXML reads the Cobertura XML and returns the raw structure and source directories.
func ParseCoberturaXML(path string) (*inputxml.CoberturaRoot, []string, error) { // Renamed from CoberturaXML to CoberturaRoot for clarity
	f, err := os.Open(path)
	if err != nil {
		return nil, nil, err
	}
	defer f.Close()

	bytes, err := io.ReadAll(f) // Read all for potential re-use or if reader needs to be reset
	if err != nil {
		return nil, nil, err
	}

	var rawReport inputxml.CoberturaRoot
	if err := xml.Unmarshal(bytes, &rawReport); err != nil { // Use Unmarshal for byte slices
		return nil, nil, err
	}

	return &rawReport, rawReport.Sources.Source, nil
}