package model

// SummaryResult is the top-level analyzed report, similar to C#'s SummaryResult
type SummaryResult struct {
	ParserName      string
	Timestamp       int64
	SourceDirs      []string
	Assemblies      []Assembly
	LinesCovered    int // Overall
	LinesValid      int // Overall
	BranchesCovered int // Overall
	BranchesValid   int // Overall
	TotalLines      int // Grand total physical lines from unique source files
	// Other overall metrics if needed
}

type Assembly struct {
	Name            string
	Classes         []Class
	LinesCovered    int
	LinesValid      int
	BranchesCovered int
	BranchesValid   int
	TotalLines      int // Sum of unique file TotalLines in this assembly
	// LineRate, BranchRate can be calculated properties or methods
}

type Class struct {
	Name            string     // The "logical" class name (e.g., Test.MyClass)
	DisplayName     string     // For display (e.g., Test.MyClass<T>)
	Files           []CodeFile // A class can span multiple files (partials)
	Methods         []Method
	LinesCovered    int
	LinesValid      int
	BranchesCovered int
	BranchesValid   int
	TotalLines      int // Sum of CodeFile.TotalLines IF class is primary owner, or complex if shared
	// LineRate, BranchRate can be calculated properties or methods
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
}

type Line struct {
	Number            int
	Hits              int
	Branch            bool
	ConditionCoverage string
}