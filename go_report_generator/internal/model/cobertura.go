package model

// CoverageReport represents the root of a Cobertura XML report
// and its main metrics and children.
type CoverageReport struct {
	LineRate        float64
	BranchRate      float64
	Version         string
	Timestamp       int64
	LinesCovered    int
	LinesValid      int
	BranchesCovered int
	BranchesValid   int
	Sources         []string
	Packages        []Package
}

type Package struct {
	Name       string
	LineRate   float64
	BranchRate float64
	Complexity float64
	Classes    []Class
}

type Class struct {
	Name       string
	Filename   string
	LineRate   float64
	BranchRate float64
	Complexity float64
	Methods    []Method
	Lines      []Line
}

type Method struct {
	Name       string
	Signature  string
	LineRate   float64
	BranchRate float64
	Complexity float64
	Lines      []Line
}

type Line struct {
	Number   int
	Hits     int
	Branch   bool
	ConditionCoverage string
}
