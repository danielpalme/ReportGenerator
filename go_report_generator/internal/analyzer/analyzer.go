package analyzer

import (
	"fmt"
	"os"
	"strconv"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

func Analyze(rawReport *inputxml.CoberturaRoot, sourceDirs []string) (*model.SummaryResult, error) {
	summary := &model.SummaryResult{
		ParserName: "Cobertura",
		SourceDirs: sourceDirs,
		Assemblies: []model.Assembly{},
		Timestamp:  processCoberturaTimestamp(rawReport.Timestamp), 
	}

	summary.LinesCovered = parseInt(rawReport.LinesCovered)
	summary.LinesValid = parseInt(rawReport.LinesValid)

	if rawReport.BranchesCovered != "" {
		val := parseInt(rawReport.BranchesCovered)
		summary.BranchesCovered = &val
	} else {
		summary.BranchesCovered = nil
	}
	if rawReport.BranchesValid != "" {
		val := parseInt(rawReport.BranchesValid)
		summary.BranchesValid = &val
	} else {
		summary.BranchesValid = nil
	}

	uniqueFilePathsForGrandTotalLines := make(map[string]int)

	for _, pkgXML := range rawReport.Packages.Package {
		assembly, err := processPackageXML(pkgXML, sourceDirs, uniqueFilePathsForGrandTotalLines)
		if err != nil {
			// Consider logging this error if a logging mechanism is introduced
			fmt.Fprintf(os.Stderr, "Warning: could not process package XML: %v\n", err)
			continue
		}
		summary.Assemblies = append(summary.Assemblies, *assembly)
	}
	
	if summary.BranchesCovered == nil && summary.BranchesValid == nil {
		var totalCovered, totalValid int
		hasAnyAssemblyBranchData := false
		for _, asm := range summary.Assemblies {
			if asm.BranchesCovered != nil && asm.BranchesValid != nil {
				hasAnyAssemblyBranchData = true
				totalCovered += *asm.BranchesCovered
				totalValid += *asm.BranchesValid
			}
		}
		if hasAnyAssemblyBranchData {
			summary.BranchesCovered = &totalCovered
			summary.BranchesValid = &totalValid
		} else {
			summary.BranchesCovered = nil
			summary.BranchesValid = nil
		}
	}

	for _, lines := range uniqueFilePathsForGrandTotalLines {
		summary.TotalLines += lines
	}

	return summary, nil
}

// UTILS

// parseInt is a utility function to parse string to int, ignoring errors for simplicity.
func parseInt(s string) int { v, _ := strconv.Atoi(s); return v }

// parseFloat is a utility function to parse string to float64, ignoring errors.
func parseFloat(s string) float64 { v, _ := strconv.ParseFloat(s, 64); return v }

// isValidUnixSeconds checks if a timestamp (in seconds) is within a reasonable range.
// E.g., between 1975-01-01 and 2100-01-01.
func isValidUnixSeconds(ts int64) bool {
	const minValidSeconds int64 = 157766400  // Approx 1975-01-01 UTC
	const maxValidSeconds int64 = 4102444800  // Approx 2100-01-01 UTC
	return ts >= minValidSeconds && ts <= maxValidSeconds
}

// processCoberturaTimestamp parses the raw timestamp string from Cobertura XML.
// It aims to extract a valid Unix timestamp in seconds.
//
// The Cobertura specification implies the timestamp should be Unix seconds.
// However, different tools produce different formats:
//   - Coverlet (C#) typically outputs Unix seconds (e.g., 10 digits).
//   - gocover-cobertura (Go) typically outputs Unix milliseconds (e.g., 13 digits).
//
// The original C# ReportGenerator, when encountering a millisecond timestamp (mistaking it for seconds),
// would calculate an erroneous far-future date, and its TextSummary report would then omit the coverage date.
// To replicate this specific output behavior (i.e., no coverage date for gocover-cobertura XMLs,
// but a date for Coverlet XMLs), this function will:
// 1. Accept and return timestamps that appear to be valid Unix seconds.
// 2. Intentionally treat timestamps that appear to be Unix milliseconds as invalid for date display (returning 0).
// 3. Return 0 for empty, unparseable, or otherwise out-of-plausible-range timestamps.
// A return value of 0 for the timestamp indicates to the reporter that no coverage date should be displayed.
func processCoberturaTimestamp(rawTimestamp string) int64 {
	if rawTimestamp == "" {
		return 0
	}

	parsedTs, err := strconv.ParseInt(rawTimestamp, 10, 64)
	if err != nil {
		return 0
	}

	timestampStrLen := len(rawTimestamp)

	if timestampStrLen >= 9 && timestampStrLen <= 11 { // Likely seconds
		if isValidUnixSeconds(parsedTs) {
			return parsedTs
		}
	} else if timestampStrLen >= 12 && timestampStrLen <= 14 { // Likely milliseconds
		// Intentionally return 0 to not display a date for these, mimicking C# RG behavior.
		return 0
	}
	
	// For any other case (unusual length, or "seconds-like" but invalid value)
	return 0
}