package main

import (
	"flag"
	"fmt"
	"os"

	"github.com/IgorBayerl/ReportGenerator/go_report_generator/internal/parser"
)

func main() {
	reportPath := flag.String("report", "", "Path to Cobertura XML file")
	flag.Parse()
	if *reportPath == "" {
		fmt.Println("Usage: go_report_generator -report <cobertura.xml>")
		os.Exit(1)
	}
	report, err := parser.ParseCobertura(*reportPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to parse: %v\n", err)
		os.Exit(1)
	}
	fmt.Printf("Cobertura Report: Lines covered: %d / %d (%.2f%%)\n", report.LinesCovered, report.LinesValid, report.LineRate*100)
	fmt.Printf("Packages: %d\n", len(report.Packages))
}
