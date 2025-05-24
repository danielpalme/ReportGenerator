package analyzer

import (
	"strconv"
	"strings"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// lineProcessingMetrics holds branch metrics for a single line.
type lineProcessingMetrics struct {
	branchesCovered int
	branchesValid   int
}

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

// processLineXML transforms an inputxml.LineXML into a model.Line and its associated metrics.
// It assumes parseInt is available (e.g. in analyzer.go or a utils package).
func processLineXML(lineXML inputxml.LineXML) (model.Line, lineProcessingMetrics) {
	metrics := lineProcessingMetrics{}
	line := model.Line{
		Number:            parseInt(lineXML.Number), 
		Hits:              parseInt(lineXML.Hits),   
		Branch:            lineXML.Branch == "true",
		ConditionCoverage: lineXML.ConditionCoverage,
	}

	if line.Branch {
		bc, bt := parseBranchCoverageForLine(line.ConditionCoverage)
		metrics.branchesCovered = bc
		metrics.branchesValid = bt
	}

	return line, metrics
}