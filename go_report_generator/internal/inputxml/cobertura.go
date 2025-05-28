package inputxml

import "encoding/xml"

// CoberturaRoot represents the root <coverage> element of a Cobertura XML file.
type CoberturaRoot struct {
	XMLName         xml.Name `xml:"coverage"`
	LineRate        string   `xml:"line-rate,attr"`
	BranchRate      string   `xml:"branch-rate,attr"`
	LinesCovered    string   `xml:"lines-covered,attr"`
	LinesValid      string   `xml:"lines-valid,attr"`
	BranchesCovered string   `xml:"branches-covered,attr"`
	BranchesValid   string   `xml:"branches-valid,attr"`
	Complexity      string   `xml:"complexity,attr"`
	Version         string   `xml:"version,attr"`
	Timestamp       string   `xml:"timestamp,attr"`
	Sources         Sources  `xml:"sources"`
	Packages        Packages `xml:"packages"`
}

// Sources represents the <sources> element.
type Sources struct {
	Source []string `xml:"source"`
}

// Packages represents the <packages> element.
type Packages struct {
	Package []PackageXML `xml:"package"`
}

// PackageXML represents a <package> element.
type PackageXML struct {
	Name       string     `xml:"name,attr"`
	LineRate   string     `xml:"line-rate,attr"`
	BranchRate string     `xml:"branch-rate,attr"`
	Complexity string     `xml:"complexity,attr"`
	Classes    ClassesXML `xml:"classes"`
}

// ClassesXML represents the <classes> element.
type ClassesXML struct {
	Class []ClassXML `xml:"class"`
}

// ClassXML represents a <class> element.
type ClassXML struct {
	Name       string     `xml:"name,attr"`
	Filename   string     `xml:"filename,attr"`
	LineRate   string     `xml:"line-rate,attr"`
	BranchRate string     `xml:"branch-rate,attr"`
	Complexity string     `xml:"complexity,attr"`
	Methods    MethodsXML `xml:"methods"`
	Lines      LinesXML   `xml:"lines"`
}

// MethodsXML represents the <methods> element.
type MethodsXML struct {
	Method []MethodXML `xml:"method"`
}

// MethodXML represents a <method> element.
type MethodXML struct {
	Name       string   `xml:"name,attr"`
	Signature  string   `xml:"signature,attr"`
	LineRate   string   `xml:"line-rate,attr"`
	BranchRate string   `xml:"branch-rate,attr"`
	Complexity string   `xml:"complexity,attr"`
	Lines      LinesXML `xml:"lines"` // Lines specific to this method
}

// LinesXML represents the <lines> element.
type LinesXML struct {
	Line []LineXML `xml:"line"`
}

// ConditionXML represents a <condition> element within <conditions>.
type ConditionXML struct {
	Number   string `xml:"number,attr"`
	Type     string `xml:"type,attr"`
	Coverage string `xml:"coverage,attr"`
}

// ConditionsXML represents the <conditions> element.
type ConditionsXML struct {
	Condition []ConditionXML `xml:"condition"`
}

// LineXML represents a <line> element.
type LineXML struct {
	Number            string        `xml:"number,attr"`
	Hits              string        `xml:"hits,attr"`
	Branch            string        `xml:"branch,attr"` // "true" or "false"
	ConditionCoverage string        `xml:"condition-coverage,attr"`
	Conditions        ConditionsXML `xml:"conditions"`
}