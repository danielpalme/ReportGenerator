package analyzer

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"time"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/filereader"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/inputxml"
	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

var genericClassRegex = regexp.MustCompile(`^(.*)\` + "`" + `(\d+)$`)
var nestedTypeSeparatorRegex = regexp.MustCompile(`[+/]`)

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
				if argCount > 1 {
					sb.WriteString(strconv.Itoa(i))
				}
			}
			sb.WriteString(">")
			genericSuffix = sb.String()
		}
	}
	return baseDisplayName + genericSuffix
}

// strips any compiler-generated part so that
//   Test.AsyncClass/<Some>d__1  →  Test.AsyncClass
//   Test.GenericAsyncClass`1+<>c  →  Test.GenericAsyncClass`1
func logicalClassName(raw string) string {
	if i := strings.IndexAny(raw, "/+"); i != -1 {
		return raw[:i]
	}
	return raw
}

// -----------------------------------------------------------------------------
// compiler-generated filter   (unchanged)
// -----------------------------------------------------------------------------

func isFilteredRawClassName(rawName string) bool {
	if strings.Contains(rawName, ">d__") ||
		strings.Contains(rawName, "/<>c") || strings.Contains(rawName, "+<>c") ||
		strings.HasPrefix(rawName, "<>c") ||
		strings.Contains(rawName, ">e__") ||
		(strings.Contains(rawName, "|") && strings.Contains(rawName, ">g__")) {
		return true
	}

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
	// explicit exclusions that the C# version hides too
	switch rawName {
	case "Test.Program/EchoHandler":
		return true
	}
	return false
}

// -----------------------------------------------------------------------------
// misc tiny helpers
// -----------------------------------------------------------------------------

func findFileInSourceDirs(relativePath string, sourceDirs []string) (string, error) {
	if filepath.IsAbs(relativePath) {
		if _, err := os.Stat(relativePath); err == nil {
			return relativePath, nil
		}
		return "", fmt.Errorf("absolute path not found: %s", relativePath)
	}
	for _, dir := range sourceDirs {
		abs := filepath.Join(filepath.Clean(dir), relativePath)
		if _, err := os.Stat(abs); err == nil {
			return abs, nil
		}
	}
	return "", fmt.Errorf("file %q not found in any source directory", relativePath)
}

func parseInt(s string) int           { v, _ := strconv.Atoi(s); return v }
func parseFloat(s string) float64     { v, _ := strconv.ParseFloat(s, 64); return v }
func parseBranchCoverage(c string) (int, int) {
	if !strings.Contains(c, "(") {
		return 0, 0
	}
	parts := strings.SplitN(c, "(", 2)
	num := strings.TrimSuffix(parts[1], ")")
	sub := strings.SplitN(num, "/", 2)
	if len(sub) != 2 {
		return 0, 0
	}
	covered, _ := strconv.Atoi(sub[0])
	total, _ := strconv.Atoi(sub[1])
	return covered, total
}

// -----------------------------------------------------------------------------
// main entry
// -----------------------------------------------------------------------------

func Analyze(rawReport *inputxml.CoberturaRoot, sourceDirs []string) (*model.SummaryResult, error) {
	summary := &model.SummaryResult{
		ParserName: "Cobertura",
		SourceDirs: sourceDirs,
		Assemblies: []model.Assembly{},
	}

	// ── time stamps ──────────────────────────────────────────────────────────
	if ts, err := strconv.ParseInt(rawReport.Timestamp, 10, 64); err == nil {
		summary.Timestamp = ts
	} else {
		summary.Timestamp = time.Now().Unix()
	}

	// ── totals from Cobertura ───────────────────────────────────────────────
	summary.LinesCovered = parseInt(rawReport.LinesCovered)
	summary.LinesValid = parseInt(rawReport.LinesValid)
	summary.BranchesCovered = parseInt(rawReport.BranchesCovered)
	summary.BranchesValid = parseInt(rawReport.BranchesValid)

	// unique physical files across *all* packages
	uniqueFilePathsForGrandTotalLines := make(map[string]int)

	// ── iterate packages (= assemblies) ─────────────────────────────────────
	for _, pkgXML := range rawReport.Packages.Package {
		assembly := model.Assembly{Name: pkgXML.Name}
		assemblyProcessedFiles := make(map[string]struct{})

		// ── iterate classes within the package ────────────────────────────
		for _, clsXML := range pkgXML.Classes.Class {
			topName := logicalClassName(clsXML.Name)
			if isFilteredRawClassName(topName) {
				continue
			}

			// look-up or create the logical class holder
			var target *model.Class
			for i := range assembly.Classes {
				if assembly.Classes[i].Name == topName {
					target = &assembly.Classes[i]
					break
				}
			}
			if target == nil {
				assembly.Classes = append(assembly.Classes, model.Class{
					Name:        topName,
					DisplayName: formatDisplayName(topName),
				})
				target = &assembly.Classes[len(assembly.Classes)-1]
			}

			// ── file-level aggregation ──────────────────────────────────
			var partLinesCovered, partLinesValid, partBranchesCovered, partBranchesValid int

			if clsXML.Filename != "" {
				codeFile := model.CodeFile{Path: clsXML.Filename}

				if resolved, err := findFileInSourceDirs(clsXML.Filename, sourceDirs); err == nil {
					codeFile.Path = resolved

					if _, known := uniqueFilePathsForGrandTotalLines[resolved]; !known {
						if n, perr := filereader.CountLinesInFile(resolved); perr == nil {
							uniqueFilePathsForGrandTotalLines[resolved] = n
							codeFile.TotalLines = n
						}
					} else {
						codeFile.TotalLines = uniqueFilePathsForGrandTotalLines[resolved]
					}
					assemblyProcessedFiles[resolved] = struct{}{}
				}

				// lines inside that <class> section
				var fileCovered, fileCoverable int
				for _, lx := range clsXML.Lines.Line {
					line := model.Line{
						Number:            parseInt(lx.Number),
						Hits:              parseInt(lx.Hits),
						Branch:            lx.Branch == "true",
						ConditionCoverage: lx.ConditionCoverage,
					}
					codeFile.Lines = append(codeFile.Lines, line)

					fileCoverable++
					partLinesValid++
					if line.Hits > 0 {
						fileCovered++
						partLinesCovered++
					}
					if line.Branch {
						bc, bt := parseBranchCoverage(line.ConditionCoverage)
						partBranchesCovered += bc
						partBranchesValid += bt
					}
				}
				codeFile.CoveredLines = fileCovered
				codeFile.CoverableLines = fileCoverable
				target.Files = append(target.Files, codeFile)
			}

			// aggregate the part we just processed
			target.LinesCovered += partLinesCovered
			target.LinesValid += partLinesValid
			target.BranchesCovered += partBranchesCovered
			target.BranchesValid += partBranchesValid

			// ── method-level aggregation ─────────────────────────────────
			for _, mx := range clsXML.Methods.Method {
				method := model.Method{
					Name:       mx.Name,
					Signature:  mx.Signature,
					Complexity: parseFloat(mx.Complexity),
				}
				var mlCovered, mlValid, mbCovered, mbValid int
				for _, lx := range mx.Lines.Line {
					l := model.Line{
						Number:            parseInt(lx.Number),
						Hits:              parseInt(lx.Hits),
						Branch:            lx.Branch == "true",
						ConditionCoverage: lx.ConditionCoverage,
					}
					method.Lines = append(method.Lines, l)
					mlValid++
					if l.Hits > 0 {
						mlCovered++
					}
					if l.Branch {
						bc, bt := parseBranchCoverage(l.ConditionCoverage)
						mbCovered += bc
						mbValid += bt
					}
				}
				if mlValid > 0 {
					method.LineRate = float64(mlCovered) / float64(mlValid)
				}
				if mbValid > 0 {
					method.BranchRate = float64(mbCovered) / float64(mbValid)
				}
				target.Methods = append(target.Methods, method)
			}
		}

		// ── assembly-level totals ──────────────────────────────────────────
		for i := range assembly.Classes {
			cls := &assembly.Classes[i]

			// physical lines per class (unique files)
			seen := make(map[string]struct{})
			for _, cf := range cls.Files {
				if cf.TotalLines > 0 {
					if _, ok := seen[cf.Path]; !ok {
						cls.TotalLines += cf.TotalLines
						seen[cf.Path] = struct{}{}
					}
				}
			}

			assembly.LinesCovered += cls.LinesCovered
			assembly.LinesValid += cls.LinesValid
			assembly.BranchesCovered += cls.BranchesCovered
			assembly.BranchesValid += cls.BranchesValid
		}

		for p := range assemblyProcessedFiles {
			assembly.TotalLines += uniqueFilePathsForGrandTotalLines[p]
		}

		summary.Assemblies = append(summary.Assemblies, assembly)
	}

	// grand total of *all* unique source files
	for _, lines := range uniqueFilePathsForGrandTotalLines {
		summary.TotalLines += lines
	}

	return summary, nil
}
