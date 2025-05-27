package analyzer

import (
	"fmt"
	"os"
	"path/filepath"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/filereader"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

// fileProcessingMetrics holds metrics aggregated during the processing of a single <class> XML element's file fragment.
type fileProcessingMetrics struct {
	linesCovered    int
	linesValid      int
	branchesCovered int
	branchesValid   int
}

// findFileInSourceDirs attempts to locate a file, first checking if it's absolute,
// then searching through the provided source directories.
func findFileInSourceDirs(relativePath string, sourceDirs []string) (string, error) {
	if filepath.IsAbs(relativePath) {
		if _, err := os.Stat(relativePath); err == nil {
			return relativePath, nil
		}
		// If absolute path not found, don't give up yet, try searching in sourceDirs
		// This handles cases where paths in Cobertura are "absolute" but only from a CI build agent's perspective
	}

	cleanedRelativePath := filepath.Clean(relativePath) 

	for _, dir := range sourceDirs {
		// Try joining directly
		abs := filepath.Join(filepath.Clean(dir), cleanedRelativePath)
		if _, err := os.Stat(abs); err == nil {
			return abs, nil
		}
	}
	return "", fmt.Errorf("file %q not found in any source directory or as absolute path", relativePath)
}

// processCodeFileFragment processes the file-specific parts of an inputxml.ClassXML.
// It creates a model.CodeFile, populates its lines, and calculates metrics for this fragment.
// It updates uniqueFilePathsForGrandTotalLines with the total line count if the file is processed for the first time.
func processCodeFileFragment(
	classXML inputxml.ClassXML,
	sourceDirs []string,
	uniqueFilePathsForGrandTotalLines map[string]int,
) (*model.CodeFile, fileProcessingMetrics, error) {
	metrics := fileProcessingMetrics{}
	codeFile := model.CodeFile{Path: classXML.Filename}
	var sourceLines []string // Declare sourceLines

	resolvedPath, err := findFileInSourceDirs(classXML.Filename, sourceDirs)
	if err == nil {
		codeFile.Path = resolvedPath // Use resolved absolute path

		// Read source file content
		sLines, readErr := filereader.ReadLinesInFile(resolvedPath)
		if readErr != nil {
			fmt.Fprintf(os.Stderr, "Warning: could not read content of source file %s: %v\n", resolvedPath, readErr)
			sourceLines = []string{} // Ensure sourceLines is empty on error
		} else {
			sourceLines = sLines
		}

		if _, known := uniqueFilePathsForGrandTotalLines[resolvedPath]; !known {
			if n, ferr := filereader.CountLinesInFile(resolvedPath); ferr == nil {
				uniqueFilePathsForGrandTotalLines[resolvedPath] = n
				codeFile.TotalLines = n
			} else {
				fmt.Fprintf(os.Stderr, "Warning: could not count lines in %s: %v\n", resolvedPath, ferr)
				// If we can't count lines, but could read them, use len(sourceLines) as a fallback for TotalLines for this specific file.
				// This might be different from physical lines if ReadLinesInFile does some normalization, but better than 0.
				if readErr == nil { // Only if ReadLinesInFile succeeded
					uniqueFilePathsForGrandTotalLines[resolvedPath] = len(sourceLines)
					codeFile.TotalLines = len(sourceLines)
				}
			}
		} else {
			codeFile.TotalLines = uniqueFilePathsForGrandTotalLines[resolvedPath]
		}
	} else {
		fmt.Fprintf(os.Stderr, "Warning: source file %s not found: %v\n", classXML.Filename, err)
		sourceLines = []string{} // Ensure sourceLines is empty if file not found
	}

	var fileFragmentCoveredLines, fileFragmentCoverableLines int

	// Process lines defined directly under <class><lines>
	for _, lineXML := range classXML.Lines.Line {
		lineModel, lineMetrics := processLineXML(lineXML, sourceLines) // Pass sourceLines
		codeFile.Lines = append(codeFile.Lines, lineModel)

		fileFragmentCoverableLines++
		metrics.linesValid++
		if lineModel.Hits > 0 {
			fileFragmentCoveredLines++
			metrics.linesCovered++
		}
		metrics.branchesCovered += lineMetrics.branchesCovered
		metrics.branchesValid += lineMetrics.branchesValid
	}
	codeFile.CoveredLines = fileFragmentCoveredLines
	codeFile.CoverableLines = fileFragmentCoverableLines

	return &codeFile, metrics, nil
}