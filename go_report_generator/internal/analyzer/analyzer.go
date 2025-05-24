package analyzer

import (
	"fmt"
	"os"
	"strconv"

	// "fmt" // For debugging, if needed
	// "os"  // For debugging, if needed

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// parseInt is a utility function to parse string to int, ignoring errors for simplicity.
func parseInt(s string) int { v, _ := strconv.Atoi(s); return v }

// parseFloat is a utility function to parse string to float64, ignoring errors.
func parseFloat(s string) float64 { v, _ := strconv.ParseFloat(s, 64); return v }

func isValidUnixSeconds(ts int64) bool {
	const minValidSeconds int64 = 157766400  // Approx 1975-01-01
	const maxValidSeconds int64 = 4102444800  // Approx 2100-01-01
	return ts >= minValidSeconds && ts <= maxValidSeconds
}

func Analyze(rawReport *inputxml.CoberturaRoot, sourceDirs []string) (*model.SummaryResult, error) {
	summary := &model.SummaryResult{
		ParserName: "Cobertura",
		SourceDirs: sourceDirs,
		Assemblies: []model.Assembly{},
		Timestamp:  0, // Default to 0
	}

	fmt.Fprintf(os.Stderr, "DEBUG: Raw Cobertura Timestamp from XML: '%s'\n", rawReport.Timestamp)

	if rawReport.Timestamp != "" {
		parsedTs, err := strconv.ParseInt(rawReport.Timestamp, 10, 64)
		if err == nil {
			fmt.Fprintf(os.Stderr, "DEBUG: Parsed ts from XML: %d\n", parsedTs)
			timestampStrLen := len(rawReport.Timestamp)
			var finalTs int64 = 0

			// C# Coverlet seems to output seconds (10 digits).
			// gocover-cobertura seems to output milliseconds (13 digits).
			// To match the C# ReportGenerator's behavior (which doesn't show a date for the Go-generated Cobertura),
			// we will only accept timestamps that look like seconds. Millisecond-looking timestamps will result in finalTs = 0.
			if timestampStrLen >= 9 && timestampStrLen <= 11 { // Likely seconds
				if isValidUnixSeconds(parsedTs) {
					finalTs = parsedTs
					fmt.Fprintf(os.Stderr, "DEBUG: Interpreted as seconds: %d\n", finalTs)
				} else {
					fmt.Fprintf(os.Stderr, "DEBUG: Looks like seconds ('%s' -> %d) but value is out of valid seconds range. Timestamp invalid.\n", rawReport.Timestamp, parsedTs)
				}
			} else if timestampStrLen >= 12 && timestampStrLen <= 14 { // Likely milliseconds, but we want to ignore these to match C# behavior for Go reports
				fmt.Fprintf(os.Stderr, "DEBUG: Timestamp '%s' looks like milliseconds. Intentionally setting Timestamp to 0 to mimic C# RG behavior for this input type.\n", rawReport.Timestamp)
				// finalTs remains 0
			} else {
				fmt.Fprintf(os.Stderr, "DEBUG: Timestamp string '%s' (parsed as %d) has unusual length. Assuming invalid.\n", rawReport.Timestamp, parsedTs)
				// finalTs remains 0
			}
			summary.Timestamp = finalTs
		} else {
			fmt.Fprintf(os.Stderr, "DEBUG: Cobertura timestamp '%s' parsing error: %v. Timestamp set to 0.\n", rawReport.Timestamp, err)
			// summary.Timestamp remains 0
		}
	} else {
		fmt.Fprintf(os.Stderr, "DEBUG: Cobertura timestamp attribute is missing/empty. Timestamp set to 0.\n")
		// summary.Timestamp remains 0
	}

	// ... (rest of the Analyze function)
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
			fmt.Fprintf(os.Stderr, "ERROR: processing package '%s': %v\n", pkgXML.Name, err)
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
			fmt.Fprintf(os.Stderr, "DEBUG: Aggregated summary branch data from assemblies: Covered=%d, Valid=%d\n", totalCovered, totalValid)
		} else {
			summary.BranchesCovered = nil
			summary.BranchesValid = nil
			fmt.Fprintf(os.Stderr, "DEBUG: No branch data found at summary or assembly level.\n")
		}
	}

	for _, lines := range uniqueFilePathsForGrandTotalLines {
		summary.TotalLines += lines
	}

	return summary, nil
}
