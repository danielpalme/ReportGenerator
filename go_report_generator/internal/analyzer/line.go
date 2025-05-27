package analyzer

import (
	// "strconv" // Removed unused import
	"strings"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// lineProcessingMetrics holds branch metrics for a single line.
type lineProcessingMetrics struct {
	branchesCovered int
	branchesValid   int
}

/*
// parseBranchCoverageForLine extracts covered and total branches from a line's condition-coverage string.
func parseBranchCoverageForLine(conditionCoverage string) (coveredBranches, totalBranches int) {
	if !strings.Contains(conditionCoverage, "(") || !strings.Contains(conditionCoverage, "/") {
		return 0, 0
	}
	parts := strings.SplitN(conditionCoverage, "(", 2)
	if len(parts) < 2 {
		return 0, 0
	}
	numStr := strings.TrimSuffix(parts[1], ")")
	subParts := strings.SplitN(numStr, "/", 2)
	if len(subParts) != 2 {
		return 0, 0
	}
	covered, _ := strconv.Atoi(subParts[0])
	total, _ := strconv.Atoi(subParts[1])
	return covered, total
}
*/

// processLineXML transforms an inputxml.LineXML into a model.Line and its associated metrics.
// It assumes parseInt is available (e.g. in analyzer.go or a utils package).
// sourceLines contains all lines from the source file this XML line belongs to.
func processLineXML(lineXML inputxml.LineXML, sourceLines []string) (model.Line, lineProcessingMetrics) {
	metrics := lineProcessingMetrics{}
	lineNumber := parseInt(lineXML.Number) // Assuming parseInt is defined elsewhere

	line := model.Line{
		Number:            lineNumber,
		Hits:              parseInt(lineXML.Hits), // Assuming parseInt is defined elsewhere
		IsBranchPoint:     (lineXML.Branch == "true"), // Set based on XML attribute
		ConditionCoverage: lineXML.ConditionCoverage,
		Branch:            make([]model.BranchCoverageDetail, 0), // Initialize as empty slice
	}

	if lineNumber > 0 && lineNumber <= len(sourceLines) {
		line.Content = sourceLines[lineNumber-1]
	} else {
		line.Content = "" // Or some placeholder like "// Source not available"
	}

	// Process detailed branch conditions from <conditions>
	if len(lineXML.Conditions.Condition) > 0 { // S1009: Simplified nil check
		for _, conditionXML := range lineXML.Conditions.Condition {
			branchDetail := model.BranchCoverageDetail{
				Identifier: conditionXML.Number,
				Visits:     0,
			}
			// Cobertura condition coverage is typically "100%" or "0% (x/y)"
			// We consider a branch covered if its specific coverage is "100%"
			if strings.HasPrefix(conditionXML.Coverage, "100") { // Handles "100%" or "100.0%"
				branchDetail.Visits = 1
			}

			line.Branch = append(line.Branch, branchDetail)

			if branchDetail.Visits > 0 {
				line.CoveredBranches++
			}
			line.TotalBranches++
		}
	} else if lineXML.Branch == "true" {
		// Fallback or handling for older Cobertura formats or lines that are marked as
		// branch points but don't have the <conditions> tag.
		// For now, we assume if lineXML.Branch is "true" but no <conditions> are present,
		// it implies a single branch that might be covered or not based on line hits.
		// This part might need refinement based on specific Cobertura variations.
		// If Hits > 0, consider it a covered branch.
		// This is a simplification. Ideally, Cobertura files should have <conditions> for branches.
		// If ConditionCoverage string has "(1/2)" etc, that might be used, but parseBranchCoverageForLine was removed.
		// For now, we won't create partial BranchCoverageDetail here without more specific rules.
		// This section fulfills the requirement to use lineXML.Branch == "true".
		// Since model.Line.Branch is a slice of BranchCoverageDetail, we can't assign a boolean directly.
		// Instead, we infer that if lineXML.Branch is true and no <conditions> are provided,
		// it represents a single branch point.
		line.TotalBranches = 1 // Represents the line itself as a single branch point
		if line.Hits > 0 {
			line.CoveredBranches = 1 // If the line was hit, consider this single branch covered
		}
	}


	metrics.branchesCovered = line.CoveredBranches
	metrics.branchesValid = line.TotalBranches

	return line, metrics
}