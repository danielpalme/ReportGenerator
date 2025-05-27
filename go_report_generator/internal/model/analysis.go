package model

// SummaryResult is the top-level analyzed report, similar to C#'s SummaryResult
type SummaryResult struct {
	ParserName      string
	Timestamp       int64
	SourceDirs      []string
	Assemblies      []Assembly
	LinesCovered    int // Overall
	LinesValid      int // Overall
	BranchesCovered *int // Overall - Pointer to indicate presence
	BranchesValid   *int // Overall - Pointer to indicate presence
	TotalLines      int // Grand total physical lines from unique source files
}

type Assembly struct {
	Name            string
	Classes         []Class
	LinesCovered    int
	LinesValid      int
	BranchesCovered *int // Pointer
	BranchesValid   *int // Pointer
	TotalLines      int  // Sum of unique file TotalLines in this assembly
}

type Class struct {
	Name            string
	DisplayName     string
	Files           []CodeFile
	Methods         []Method
	LinesCovered    int
	LinesValid      int
	BranchesCovered *int // Pointer
	BranchesValid   *int // Pointer
	TotalLines      int
	HistoricCoverages []HistoricCoverage // Historical coverage data for this class
}

type CodeFile struct {
	Path           string
	Lines          []Line // Coverage data for lines in this file
	CoveredLines   int    // Specific to this file's part
	CoverableLines int    // Specific to this file's part
	TotalLines     int    // Total physical lines in this source file
	// Branches specific to this file's part
}

type Method struct {
	Name       string
	Signature  string
	LineRate   float64 // Stored as 0-1.0 
	BranchRate float64 // Stored as 0-1.0
	Complexity float64
	Lines      []Line
	FirstLine     int            // The line number where the method definition starts
	LastLine      int            // The line number where the method definition ends
	MethodMetrics []MethodMetric // Specific metrics for this method
}

// BranchCoverageDetail provides details about a specific branch on a line.
type BranchCoverageDetail struct {
	Identifier string // Unique identifier for the branch, e.g., "0", "1", "true", "false"
	Visits     int    // Number of times this specific branch was visited
}

type Line struct {
	Number                 int
	Hits                   int
	IsBranchPoint          bool                   // True if the line is a branch point (from XML branch="true")
	Branch                 []BranchCoverageDetail // Details of branches on this line
	ConditionCoverage      string
	Content                string            // The actual source code content of the line
	CoveredBranches        int               // Number of branches on this line that were covered
	TotalBranches          int               // Total number of branches on this line
	LineCoverageByTestMethod map[string]int    // Tracks hits for this line by TestMethod.ID
}