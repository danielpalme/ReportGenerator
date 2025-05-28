package model

// HistoricCoverage represents historical code coverage data.
type HistoricCoverage struct {
	ExecutionTime  int64
	Tag            string
	CoveredLines   int
	CoverableLines int
	TotalLines     int
	CoveredBranches int
	TotalBranches  int
}
