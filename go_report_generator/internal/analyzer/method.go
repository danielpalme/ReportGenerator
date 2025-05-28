package analyzer

import (
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"math"

	// "github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml" // Removed duplicate
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// processMethodXML processes an inputxml.MethodXML and transforms it into a model.Method.
// sourceLines are the lines of the source file this method belongs to.
func processMethodXML(methodXML inputxml.MethodXML, sourceLines []string) (*model.Method, error) {
	method := model.Method{
		Name:       methodXML.Name,
		Signature:  methodXML.Signature,
		Complexity: parseFloat(methodXML.Complexity), // Keep direct complexity for now
	}

	var methodLinesCovered, methodLinesValid int
	var methodBranchesCovered, methodBranchesValid int

	minLine := math.MaxInt32 // Initialize minLine to a very large number
	maxLine := 0             // Initialize maxLine to 0

	// Process lines specific to this method
	for _, lineXML := range methodXML.Lines.Line {
		currentLineNum := parseInt(lineXML.Number)

		if currentLineNum < minLine {
			minLine = currentLineNum
		}
		if currentLineNum > maxLine {
			maxLine = currentLineNum
		}

		lineModel, lineMetrics := processLineXML(lineXML, sourceLines) // Pass sourceLines
		method.Lines = append(method.Lines, lineModel)

		methodLinesValid++
		if lineModel.Hits > 0 {
			methodLinesCovered++
		}
		methodBranchesCovered += lineMetrics.branchesCovered
		methodBranchesValid += lineMetrics.branchesValid
	}

	if minLine == math.MaxInt32 { // No lines found
		method.FirstLine = 0
		// minLine = 0 // Removed ineffectual assignment (ineffassign)
	} else {
		method.FirstLine = minLine
	}
	method.LastLine = maxLine

	// Populate MethodMetrics for Complexity
	complexityValue := parseFloat(methodXML.Complexity)
	complexityMetric := model.Metric{
		Name:   "Complexity",
		Value:  complexityValue,
		Status: model.StatusOk, // Assuming model.StatusOk is defined (e.g., as 0 or an iota const)
	}
	methodComplexityMetric := model.MethodMetric{
		Name:    "Cyclomatic Complexity", // More specific name
		Line:    method.FirstLine,        // Associate with the start of the method
		Metrics: []model.Metric{complexityMetric},
	}
	method.MethodMetrics = []model.MethodMetric{methodComplexityMetric}

	if methodLinesValid > 0 {
		method.LineRate = float64(methodLinesCovered) / float64(methodLinesValid)
	}
	if methodBranchesValid > 0 {
		method.BranchRate = float64(methodBranchesCovered) / float64(methodBranchesValid)
	}

	return &method, nil
}