# Go Report Generator – Cobertura Migration Checklist  

## ✅ Completed (Cobertura-specific)

- [x] Modular Go project layout (`cmd/`, `internal/*`).
- [x] Cobertura **raw data model** (`internal/inputxml` structs).
- [x] Cobertura **XML parser** (`internal/parser`).
- [x] Enriched **analysis layer** (`internal/analyzer`) producing `SummaryResult`.
- [x] Unified **domain model** (`internal/model`) mirroring C# structures.
- [x] **TextSummary** reporter that replicates C# “Summary.txt”.
- [x] **CLI** with flags `-report`, `-output`, `-reporttypes` + validation.
- [x] Builds & runs with `go run ./cmd` or `go build ./...`.

## 🟡 In Progress / To-Do (Cobertura focus)

| Area | What’s left |
|------|-------------|
| **Testing** | • Unit tests for XML parser and analyzer.<br>• Golden-file tests for TextSummary output. |
| **Robustness** | • Centralised error & warning logger.<br>• Graceful handling of malformed XML. |
| **Reporting** | • HTML report builder matching C# ReportGenerator.<br>• Optionally emit JSON for downstream tools. |
| **CLI UX** | • `-v/--verbose` flag (log level).<br>• `--reporttypes Html,Json,…` auto-detect duplicates. |
| **Automation** | • Script to run sample projects → `go test -coverprofile` → `gocover-cobertura` → compare output.<br>• Python diff utility integration. |
| **Docs** | • Expand README with advanced examples.<br>• Developer guide for adding new formats. |

## 🔮 Future-Proofing (Multi-format)

- [ ] Abstract `Parser` interface (Cobertura, OpenCover, JaCoCo, …).
- [ ] Plug-in discovery (build-tag or go:generate).
- [ ] Auto-detect report format from XML root or explicit flag.
- [ ] Ensure domain model can express unique features of other formats.
- [ ] Shared test-suite covering every supported format.
