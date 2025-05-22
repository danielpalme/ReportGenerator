# Go Report Generator: Cobertura Migration Checklist

## ✅ Features Already Implemented

- [x] Project structure: Modular Go project with `internal/model`, `internal/parser`, and `cmd/`.
- [x] Cobertura data model: Go structs for CoverageReport, Package, Class, Method, Line.
- [x] Cobertura XML parser: Reads and parses Cobertura XML into Go structs.
- [x] Basic CLI: Command-line tool that loads a Cobertura XML and prints a summary (lines covered, total lines, package count).
- [x] Builds successfully: Project compiles and runs with the above features.

## 🟡 To-Do List (Cobertura-focused)

- [ ] Unit tests for parser: Add Go tests to validate parsing against real Cobertura XML files.
- [ ] Error handling: Improve error messages and robustness for malformed or incomplete XML.
- [ ] Report output: Generate HTML or other human-readable reports from the parsed data (to match C# output).
- [ ] CLI options: Add flags for output directory, report format, and verbosity.
- [ ] Integration with test projects: Automate running test projects and generating Cobertura XML for comparison.
- [ ] Comparison automation: Integrate with the Python compare script for automated validation.
- [ ] Documentation: Add usage instructions and developer docs.

## 🟦 Future-Proofing for Other Report Formats

- [ ] Abstract parser interface: Define a common interface for report parsers (e.g., Cobertura, OpenCover, JaCoCo, etc.).
- [ ] Pluggable parser architecture: Allow easy addition of new report formats by implementing the interface.
- [ ] Format auto-detection: Detect report type from XML root or CLI flag.
- [ ] Unified data model: Ensure the internal model can represent all supported formats’ features.
- [ ] Comprehensive test suite: Add test data and tests for each supported format.
