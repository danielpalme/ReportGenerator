package analyzer

import (
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// processMethodXML processes an inputxml.MethodXML and transforms it into a model.Method.
func processMethodXML(methodXML inputxml.MethodXML, containingClassFilename string) (*model.Method, error) {
	method := model.Method{
		Name:       methodXML.Name,
		Signature:  methodXML.Signature,
		Complexity: parseFloat(methodXML.Complexity), 
	}

	var methodLinesCovered, methodLinesValid int
	var methodBranchesCovered, methodBranchesValid int

	// Process lines specific to this method
	for _, lineXML := range methodXML.Lines.Line {
		lineModel, lineMetrics := processLineXML(lineXML) 
		method.Lines = append(method.Lines, lineModel)

		methodLinesValid++
		if lineModel.Hits > 0 {
			methodLinesCovered++
		}
		methodBranchesCovered += lineMetrics.branchesCovered
		methodBranchesValid += lineMetrics.branchesValid
	}

	if methodLinesValid > 0 {
		method.LineRate = float64(methodLinesCovered) / float64(methodLinesValid)
	}
	if methodBranchesValid > 0 {
		method.BranchRate = float64(methodBranchesCovered) / float64(methodBranchesValid)
	}

	return &method, nil
}