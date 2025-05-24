// internal/analyzer/assembly.go
package analyzer

import (
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

func processPackageXML(pkgXML inputxml.PackageXML, sourceDirs []string, uniqueFilePathsForGrandTotalLines map[string]int) (*model.Assembly, error) {
	assembly := model.Assembly{
		Name:    pkgXML.Name,
		Classes: []model.Class{},
		// BranchesCovered and BranchesValid will be nil initially
	}
	assemblyProcessedFilePaths := make(map[string]struct{})

	classesXMLGrouped := make(map[string][]inputxml.ClassXML)
	for _, clsXML := range pkgXML.Classes.Class {
		logicalName := logicalClassName(clsXML.Name)
		classesXMLGrouped[logicalName] = append(classesXMLGrouped[logicalName], clsXML)
	}

	for logicalName, classXMLGroup := range classesXMLGrouped {
		if isFilteredRawClassName(logicalName) {
			continue
		}
		classModel, err := processClassGroup(classXMLGroup, assembly.Name, sourceDirs, uniqueFilePathsForGrandTotalLines, assemblyProcessedFilePaths)
		if err != nil {
			continue
		}
		if classModel != nil {
			assembly.Classes = append(assembly.Classes, *classModel)
		}
	}

	// Aggregate assembly-level metrics
	var totalAsmBranchesCovered, totalAsmBranchesValid int
	hasAsmBranchData := false

	for i := range assembly.Classes {
		cls := &assembly.Classes[i]
		assembly.LinesCovered += cls.LinesCovered
		assembly.LinesValid += cls.LinesValid

		if cls.BranchesCovered != nil && cls.BranchesValid != nil {
			hasAsmBranchData = true
			totalAsmBranchesCovered += *cls.BranchesCovered
			totalAsmBranchesValid += *cls.BranchesValid
		}
	}

	if hasAsmBranchData {
		assembly.BranchesCovered = &totalAsmBranchesCovered
		assembly.BranchesValid = &totalAsmBranchesValid
	}


	for path := range assemblyProcessedFilePaths {
		if lineCount, ok := uniqueFilePathsForGrandTotalLines[path]; ok {
			assembly.TotalLines += lineCount
		}
	}

	return &assembly, nil
}