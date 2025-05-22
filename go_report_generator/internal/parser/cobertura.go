package parser

import (
	"encoding/xml"
	"io"
	"os"
	"strconv"
	"strings"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/model"
)

type coberturaXML struct {
	XMLName         xml.Name `xml:"coverage"`
	LineRate        string   `xml:"line-rate,attr"`
	BranchRate      string   `xml:"branch-rate,attr"`
	Version         string   `xml:"version,attr"`
	Timestamp       string   `xml:"timestamp,attr"`
	LinesCovered    string   `xml:"lines-covered,attr"`
	LinesValid      string   `xml:"lines-valid,attr"`
	BranchesCovered string   `xml:"branches-covered,attr"`
	BranchesValid   string   `xml:"branches-valid,attr"`
	Sources         struct {
		Source []string `xml:"source"`
	} `xml:"sources"`
	Packages struct {
		Package []packageXML `xml:"package"`
	} `xml:"packages"`
}

type packageXML struct {
	Name       string      `xml:"name,attr"`
	LineRate   string      `xml:"line-rate,attr"`
	BranchRate string      `xml:"branch-rate,attr"`
	Complexity string      `xml:"complexity,attr"`
	Classes    struct {
		Class []classXML `xml:"class"`
	} `xml:"classes"`
}

type classXML struct {
	Name       string      `xml:"name,attr"`
	Filename   string      `xml:"filename,attr"`
	LineRate   string      `xml:"line-rate,attr"`
	BranchRate string      `xml:"branch-rate,attr"`
	Complexity string      `xml:"complexity,attr"`
	Methods    struct {
		Method []methodXML `xml:"method"`
	} `xml:"methods"`
	Lines struct {
		Line []lineXML `xml:"line"`
	} `xml:"lines"`
}

type methodXML struct {
	Name       string    `xml:"name,attr"`
	Signature  string    `xml:"signature,attr"`
	LineRate   string    `xml:"line-rate,attr"`
	BranchRate string    `xml:"branch-rate,attr"`
	Complexity string    `xml:"complexity,attr"`
	Lines      struct {
		Line []lineXML `xml:"line"`
	} `xml:"lines"`
}

type lineXML struct {
	Number            string `xml:"number,attr"`
	Hits              string `xml:"hits,attr"`
	Branch            string `xml:"branch,attr"`
	ConditionCoverage string `xml:"condition-coverage,attr"`
}

// ParseCobertura parses a Cobertura XML file and returns a CoverageReport model
func ParseCobertura(path string) (*model.CoverageReport, error) {
	f, err := os.Open(path)
	if err != nil {
		return nil, err
	}
	defer f.Close()
	return parseCoberturaReader(f)
}

func parseCoberturaReader(r io.Reader) (*model.CoverageReport, error) {
	var xmlReport coberturaXML
	if err := xml.NewDecoder(r).Decode(&xmlReport); err != nil {
		return nil, err
	}
	return convertCobertura(xmlReport)
}

func convertCobertura(x coberturaXML) (*model.CoverageReport, error) {
	parseF := func(s string) float64 {
		f, _ := strconv.ParseFloat(s, 64)
		return f
	}
	parseI := func(s string) int {
		i, _ := strconv.Atoi(s)
		return i
	}
	report := &model.CoverageReport{
		LineRate:        parseF(x.LineRate),
		BranchRate:      parseF(x.BranchRate),
		Version:         x.Version,
		Timestamp:       int64(parseI(x.Timestamp)),
		LinesCovered:    parseI(x.LinesCovered),
		LinesValid:      parseI(x.LinesValid),
		BranchesCovered: parseI(x.BranchesCovered),
		BranchesValid:   parseI(x.BranchesValid),
		Sources:         x.Sources.Source,
	}
	for _, pkg := range x.Packages.Package {
		p := model.Package{
			Name:       pkg.Name,
			LineRate:   parseF(pkg.LineRate),
			BranchRate: parseF(pkg.BranchRate),
			Complexity: parseF(pkg.Complexity),
		}
		for _, cls := range pkg.Classes.Class {
			c := model.Class{
				Name:       cls.Name,
				Filename:   cls.Filename,
				LineRate:   parseF(cls.LineRate),
				BranchRate: parseF(cls.BranchRate),
				Complexity: parseF(cls.Complexity),
			}
			for _, m := range cls.Methods.Method {
				meth := model.Method{
					Name:       m.Name,
					Signature:  m.Signature,
					LineRate:   parseF(m.LineRate),
					BranchRate: parseF(m.BranchRate),
					Complexity: parseF(m.Complexity),
				}
				for _, l := range m.Lines.Line {
					meth.Lines = append(meth.Lines, model.Line{
						Number:            parseI(l.Number),
						Hits:              parseI(l.Hits),
						Branch:            strings.ToLower(l.Branch) == "true",
						ConditionCoverage: l.ConditionCoverage,
					})
				}
				c.Methods = append(c.Methods, meth)
			}
			for _, l := range cls.Lines.Line {
				c.Lines = append(c.Lines, model.Line{
					Number:            parseI(l.Number),
					Hits:              parseI(l.Hits),
					Branch:            strings.ToLower(l.Branch) == "true",
					ConditionCoverage: l.ConditionCoverage,
				})
			}
			p.Classes = append(p.Classes, c)
		}
		report.Packages = append(report.Packages, p)
	}
	return report, nil
}
