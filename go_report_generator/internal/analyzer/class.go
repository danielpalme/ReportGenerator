package analyzer

import (
	"fmt"
	"os" // Ensure os is imported
	"regexp"
	"strconv"
	"strings"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/filereader" // Ensure filereader is imported
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// Regex to handle .NET-style generic class names (e.g., MyClass`1)
var genericClassRegex = regexp.MustCompile(`^(.*)\` + "`" + `(\d+)$`)

// Regex to replace .NET nested class separators (+ or /) with a dot for display.
var nestedTypeSeparatorRegex = regexp.MustCompile(`[+/]`)

// formatDisplayName creates a human-readable class name, handling generics like C#.
// E.g., "Namespace.MyClass`1" becomes "Namespace.MyClass<T>"
// E.g., "Namespace.Outer/Nested" becomes "Namespace.Outer.Nested"
func formatDisplayName(rawCoberturaClassName string) string {
	nameForDisplay := nestedTypeSeparatorRegex.ReplaceAllString(rawCoberturaClassName, ".")
	match := genericClassRegex.FindStringSubmatch(nameForDisplay)

	baseDisplayName := nameForDisplay
	var genericSuffix string
	if match != nil {
		baseDisplayName = match[1]
		if argCount, _ := strconv.Atoi(match[2]); argCount > 0 {
			var sb strings.Builder
			sb.WriteString("<")
			for i := 1; i <= argCount; i++ {
				if i > 1 {
					sb.WriteString(", ")
				}
				sb.WriteString("T")
				if argCount > 1 { // Append number only if more than one generic arg
					sb.WriteString(strconv.Itoa(i))
				}
			}
			sb.WriteString(">")
			genericSuffix = sb.String()
		}
	}
	return baseDisplayName + genericSuffix
}

// logicalClassName strips compiler-generated parts from a raw class name to get a "logical" grouping key.
// E.g., "Test.AsyncClass/<Some>d__1"  -> "Test.AsyncClass"
// E.g., "Test.GenericAsyncClass`1+<>c" -> "Test.GenericAsyncClass`1"
func logicalClassName(raw string) string {
	// This finds the first occurrence of '/' or '+' which often delimit compiler-generated parts or nested types.
	if i := strings.IndexAny(raw, "/+"); i != -1 {
		// Check if the part after / or + looks like a C# compiler-generated suffix
		// This is a heuristic. A more robust solution might involve more specific regexes
		// if this proves insufficient across different Cobertura generators.
		// For now, the original logic is preserved.
		return raw[:i]
	}
	return raw
}

// isFilteredRawClassName checks if a raw class name matches patterns for common compiler-generated types
// that are typically excluded from reports.
func isFilteredRawClassName(rawName string) bool {
	// Original C# filtering logic for .NET compiler-generated types
	if strings.Contains(rawName, ">d__") || // Common for async state machines
		strings.Contains(rawName, "/<>c") || strings.Contains(rawName, "+<>c") || // Common for display classes holding lambdas
		strings.HasPrefix(rawName, "<>c") || // Standalone display class
		strings.Contains(rawName, ">e__") || // Iterator state machines
		(strings.Contains(rawName, "|") && strings.Contains(rawName, ">g__")) { // Local functions
		return true
	}

	// Check for nested compiler-generated types
	if idx := strings.LastIndexAny(rawName, "/+"); idx != -1 {
		nestedPart := rawName[idx+1:]
		if strings.HasPrefix(nestedPart, "<") &&
			(strings.Contains(nestedPart, ">d__") ||
				strings.Contains(nestedPart, ">e__") ||
				strings.Contains(nestedPart, ">g__")) {
			return true
		}
		if strings.HasPrefix(nestedPart, "<>c") {
			return true
		}
	}

	// This was an explicit exclusion in the original C# code.
	// It might be language/project specific.
	switch rawName {
	case "Test.Program/EchoHandler": // Example, adjust if needed
		return true
	}

	return false
}

// processClassGroup processes a group of inputxml.ClassXML elements that belong to the same logical class.
// It creates a single model.Class, aggregating data from all provided XML fragments.
// It updates uniqueFilePathsForGrandTotalLines and assemblyProcessedFilePaths.
func processClassGroup(
	classXMLs []inputxml.ClassXML,
	assemblyName string,
	sourceDirs []string,
	uniqueFilePathsForGrandTotalLines map[string]int,
	assemblyProcessedFilePaths map[string]struct{},
) (*model.Class, error) {
	if len(classXMLs) == 0 {
		return nil, nil
	}

	logicalName := logicalClassName(classXMLs[0].Name)
	displayName := formatDisplayName(logicalName)

	classModel := model.Class{
		Name:        logicalName,
		DisplayName: displayName,
		Files:       []model.CodeFile{},
		Methods:     []model.Method{},
		// BranchesCovered and BranchesValid will be nil
	}
	classProcessedFilePaths := make(map[string]struct{})

	var totalClassBranchesCovered, totalClassBranchesValid int
	hasClassBranchData := false

	for _, clsXML := range classXMLs {
		if clsXML.Filename == "" {
			continue
		}

		codeFile, fragmentMetrics, err := processCodeFileFragment(clsXML, sourceDirs, uniqueFilePathsForGrandTotalLines)
		if err != nil {
			continue
		}
		if codeFile != nil {
			// Simplified file merging (assumes distinct file fragments per clsXML or non-overlapping lines)
			existingFileIndex := -1
			for i := range classModel.Files {
				if classModel.Files[i].Path == codeFile.Path {
					existingFileIndex = i
					break
				}
			}
			if existingFileIndex != -1 {
				classModel.Files[existingFileIndex].CoveredLines += codeFile.CoveredLines
				classModel.Files[existingFileIndex].CoverableLines += codeFile.CoverableLines
				classModel.Files[existingFileIndex].Lines = append(classModel.Files[existingFileIndex].Lines, codeFile.Lines...)
			} else {
				classModel.Files = append(classModel.Files, *codeFile)
			}

			assemblyProcessedFilePaths[codeFile.Path] = struct{}{}
			classProcessedFilePaths[codeFile.Path] = struct{}{}
		}

		classModel.LinesCovered += fragmentMetrics.linesCovered
		classModel.LinesValid += fragmentMetrics.linesValid

		if fragmentMetrics.branchesValid > 0 || fragmentMetrics.branchesCovered > 0 { // Or more strictly, if the XML attribute was present
			hasClassBranchData = true
			totalClassBranchesCovered += fragmentMetrics.branchesCovered
			totalClassBranchesValid += fragmentMetrics.branchesValid
		}

		// Read source file content for the current class fragment (clsXML.Filename)
		var classFragmentSourceLines []string
		if clsXML.Filename != "" { // Only attempt if filename is present
			resolvedPath, findErr := findFileInSourceDirs(clsXML.Filename, sourceDirs)
			if findErr == nil {
				sLines, readErr := filereader.ReadLinesInFile(resolvedPath)
				if readErr == nil {
					classFragmentSourceLines = sLines
				} else {
					fmt.Fprintf(os.Stderr, "Warning: could not read content of source file %s for class %s: %v\n", resolvedPath, clsXML.Name, readErr)
					classFragmentSourceLines = []string{}
				}
			} else {
				// This warning might be redundant if processCodeFileFragment already warned.
				// However, methods for this clsXML won't get source lines if the file isn't found here.
				fmt.Fprintf(os.Stderr, "Warning: source file %s for class %s not found when preparing for method processing: %v\n", clsXML.Filename, clsXML.Name, findErr)
				classFragmentSourceLines = []string{}
			}
		} else {
			classFragmentSourceLines = []string{} // Ensure it's empty if no filename
		}

		for _, methodXML := range clsXML.Methods.Method {
			method, err := processMethodXML(methodXML, classFragmentSourceLines) // Pass classFragmentSourceLines
			if err != nil {
				// Consider logging this error or handling it more explicitly
				fmt.Fprintf(os.Stderr, "Warning: error processing method %s in class %s: %v\n", methodXML.Name, clsXML.Name, err)
				continue
			}
			classModel.Methods = append(classModel.Methods, *method)
			// Aggregate method branch data if methods also have branch pointers
			// For now, method branch data contributes to class via fragmentMetrics
		}
	}
	
	if hasClassBranchData {
		classModel.BranchesCovered = &totalClassBranchesCovered
		classModel.BranchesValid = &totalClassBranchesValid
	}


	for path := range classProcessedFilePaths {
		if lineCount, ok := uniqueFilePathsForGrandTotalLines[path]; ok {
			classModel.TotalLines += lineCount
		}
	}

	return &classModel, nil
}