# MASTER_PLAN.md: Porting ReportGenerator HTML Reporting to Go

## 🎯 Overall Project Goal
To port the HTML reporting functionality from the C# ReportGenerator tool to the Go version (`go_report_generator`). The initial focus is on processing Cobertura XML input and generating rich HTML reports, aiming for feature parity with the original C# tool's HTML output. The architecture should allow for future expansion to other report types and input formats.

---
## 📊 Current Status (Go Project)
* **Input Parsing:** Successfully parses Cobertura XML files.
* **Internal Data Model:** An internal data model exists, populated from the Cobertura input.
* **Reporting Engine:**
    * Basic reporter interface (`ReportBuilder`) is defined.
    * `TextSummary` report generation is implemented and functional.
* **CLI:** Basic command-line interface accepts input XML and report type.
---
## leveragedLeverage (Go Port vs. C# Original)
* **Cobertura Parsing (Go: Done):**
    `go_report_generator/internal/parser/cobertura.go` parses the XML into `inputxml.CoberturaRoot`.
    This is analogous to C#'s `CoberturaParser.cs`.
* **Internal Data Model (Go: Done, Needs Verification for Completeness):**
    `go_report_generator/internal/model/*` (e.g., `analysis.go`, `metric.go`, etc.) defines structs like `SummaryResult`, `Assembly`, `Class`, `CodeFile`, `Method`, `Line`, `Metric`, `CodeElement`.
    This is analogous to C#'s `ReportGenerator.Core/Parser/Analysis/*`.
    *Action:* Verify if all necessary fields from C#'s analysis model (especially those used by HTML rendering like `HistoricCoverage`, `TestMethod` links, detailed branch info per line) are present or can be easily added to the Go model. Your `model/analysis.go Line` struct already has `CoveredBranches` and `TotalBranches`, which is great. `HistoricCoverage` is also present.
* **Analyzer (Go: Done, Needs Verification for Completeness):**
    `go_report_generator/internal/analyzer/*` populates the Go internal data model from `inputxml.CoberturaRoot`.
    This is analogous to the processing logic within C# parsers that hydrate `Parser.Analysis.Assembly`, `Parser.Analysis.Class`, etc.
    *Action:* Similar to the data model, ensure the analyzer populates all data points required for the rich HTML report.
* **Reporter Abstraction (Go: Done):**
    `go_report_generator/internal/reporter/reporter.go` defines the `ReportBuilder` interface.
    `go_report_generator/internal/reporter/textsummary/reporter.go` is an existing implementation. This provides a pattern.
* **Angular SPA (C#: Can be directly COPIED and BUILT):**
    The entire `src/AngularComponents` directory from the C# project.
    Its build process (`ng build` defined in `angular.json`) produces static assets (JS, CSS, `index.html` shell).
    *Key Insight:* The Go backend does not *run* Angular. It will *serve* or *integrate* the pre-compiled static assets from Angular's `dist` folder.
* **Static Assets (CSS/JS) (C#: Can be directly COPIED):**
    Files in `src/ReportGenerator.Core/Reporting/Builders/Rendering/resources/` (e.g., `custom.css`, `custom.js`, `chartist.min.css/js`) can be copied into an `assets/` directory in the Go project.
* **HTML Rendering Logic (C#: Needs TRANSLATION to Go):**
    C#'s `HtmlRenderer.cs` and `HtmlReportBuilderBase.cs` contain the core logic for:
    * Generating HTML structure (summary page, class detail pages).
    * Iterating through the data model to populate tables, lists, metrics.
    * Creating links between summary and detail pages.
    * Embedding or linking static/Angular assets.
    * Generating JavaScript data variables for the Angular app.
    This logic will be translated into Go, primarily using the `html/template` package and Go string manipulation.
* **Source File Reading (Go: Partially Exists, Needs Integration):**
    `go_report_generator/internal/filereader/filereader.go` exists for counting lines. It needs to be enhanced or complemented to read file content for display.
    C#'s `SourceFileRenderer.cs` (or similar logic within `HtmlRenderer`) handles reading source files and applying CSS classes. This part needs translation to Go.

---
## 🗺️ High-Level HTML Reporting Plan
1.  ✅ **Foundation & Asset Integration:** Set up the basic HTML report builder, integrate static CSS/JS assets, and incorporate the pre-compiled Angular SPA.
2.  **Data-Driven Rendering:** Implement Go backend logic to provide data (summary, class details, source code, history) to the Angular SPA for rendering dynamic HTML pages.
3.  **Feature Parity & Refinements:** Add advanced features (inline HTML, risk hotspots, theming) and polish the output.
4.  **Finalization:** Thorough testing, performance optimization, and documentation.

---
## 🛠️ Detailed Plan for HTML Report Implementation (from Cobertura XML)

### Phase 0: Pre-flight Checks & Setup
* ✅ **Data Model & Analyzer Review (Go vs. C#):**
    * **Task:** Compare Go's `internal/model` structs and `internal/analyzer` logic with C#'s `ReportGenerator.Core/Parser/Analysis` classes.
    * **Goal:** Ensure all data points rendered in the C# HTML report (summary stats, per-class stats, line details, branch details, method metrics, historic coverage) can be populated in the Go model.
    * **C# Reference:** `src/ReportGenerator.Core/Parser/Analysis/*`
    * **Status:** Largely complete, ongoing verification.
* ✅ **Command Line Argument for HTML Report (Go):**
    * **Task:** Modify `cmd/main.go` to accept "Html" (and potentially "HtmlInline" later) as a `reporttypes` argument.
    * **Goal:** Allow users to request HTML report generation.
    * **C# Reference:** N/A (CLI parsing logic).
    * **Status:** Implemented for "Html".

---
### Phase 1: Basic HTML Structure & Asset Management
* **Step 1.1: HTML Report Builder and Basic Templates (Go)**
    * **Goal:** Create the foundational Go package and files for HTML report generation, capable of producing a basic, valid `index.html` shell.
    * **Tasks:**
        * Create `go_report_generator/internal/reporter/htmlreport/` package.
        * Define `HtmlReportBuilder` struct implementing `reporter.ReportBuilder`.
        * Integrate `HtmlReportBuilder` into `cmd/main.go`.
        * Define basic Go `html/template` for the main HTML shell (`base_layout.gohtml`), referencing C#'s `HtmlRenderer.WriteHtmlStart()` for structure.
        * Implement `CreateReport` in `HtmlReportBuilder` to generate `output/index.html`.
    * **C# Reference:** `src/ReportGenerator.Core/Reporting/Builders/Rendering/HtmlRenderer.cs`
    * **Status:** DONE.
* **Step 1.2: Copy and Integrate Static Assets (CSS/JS from C# `resources/`)**
    * **Goal:** Link non-Angular custom CSS (e.g., `custom.css`, themes) and JavaScript files (e.g., `custom.js`, `chartist.min.js`) used by the original ReportGenerator.
    * **Tasks:**
        * Create `go_report_generator/assets/htmlreport/`.
        * Copy C# static assets to `assets/htmlreport/`.
        * Implement asset copying logic in `htmlreport/builder.go` to move assets to the `output/` directory.
        * Combine `custom.css` and theme CSS into `output/report.css`.
        * Update `base_layout.gohtml` to link these CSS and JS files.
    * **C# Reference:** `src/ReportGenerator.Core/Reporting/Builders/Rendering/resources/*`, `HtmlRenderer.SaveCss`
    * **Status:** DONE.
    * **Summary:** Created `go_report_generator/assets/htmlreport/`, copied relevant CSS/JS files from C# (`custom.css`, `custom.js`, `chartist.min.css`, `chartist.min.js`, and theme files). Implemented logic in `HtmlReportBuilder` to copy these to the output. Combined `custom.css` and `custom_dark.css` (as a default theme example) into `report.css`. Updated `base_layout.gohtml` to link `report.css`, `chartist.min.css`, `custom.js`, and `chartist.min.js`. Verified by generating a sample report.
* **Step 1.3: Integrate Pre-compiled Angular SPA Assets**
    * **Goal:** Integrate the static files (JS, CSS, assets) from the pre-compiled Angular SPA into the Go build process and output directory.
    * **Tasks:**
        * Copy C#'s `src/AngularComponents` to `go_report_generator/angular_frontend_spa/`.
        * Establish a build process for the Angular SPA (`npm install && npm run build_prod`).
        * Implement logic in `htmlreport/builder.go` to copy Angular's `dist/` output to the Go `output/` directory.
        * Update `base_layout.gohtml` to correctly link Angular's JS/CSS bundles (e.g., `styles.*.css`, `runtime.*.js`, `main.*.js`).
    * **C# Reference:** `src/AngularComponents/`, `ReportGenerator.Core.csproj` (for EmbeddedResource), `HtmlRenderer.cs` (for script/style linking).
    * **Status:** DONE.
    * **Summary (Step 1.3):**
        * Copied `src/AngularComponents` content to `go_report_generator/angular_frontend_spa/` (this directory contains the AngularComponents source).
        * **Developer Note:** The Angular SPA must be built manually by the developer. Navigate to `go_report_generator/angular_frontend_spa/` and run `npm install` (once) and then `npm run build`. This will generate the necessary static assets in `go_report_generator/angular_frontend_spa/dist/`.
        * Updated `go_report_generator/internal/reporter/htmlreport/builder.go`:
            * Added `copyAngularAssets` function to copy all files and subdirectories from `angular_frontend_spa/dist/coverage-app/browser/` (once built by the developer) to the Go report output directory.
            * Implemented `parseAngularIndexHTML` to read the Angular-generated `index.html` (from the developer's build) and extract hashed JS/CSS filenames.
            * The Go `CreateReport` function passes these filenames to the HTML template.
        * Updated `go_report_generator/internal/reporter/htmlreport/templates.go`:
            * Modified `baseLayoutTemplate` to dynamically link the Angular CSS and JS files using the extracted names.
            * Added `<app-root></app-root>` to the template for Angular bootstrapping.
        * Placeholder Angular asset files (previously used for simulation) have been removed.
        * Created `go_report_generator/angular_frontend_spa/.gitignore` to exclude `dist/` and `node_modules/` from Git tracking.
        * The `Testprojects/generate_reports.py` script contains a comment reminding about the manual Angular build step.

---
### Phase 2: Rendering Data with Angular (Go Backend Provides Data)
* **Step 2.1: Summary Page with Angular Rendering**
    * **Goal:** The `output/index.html` (summary page) should be fully rendered by the Angular SPA, using data provided by Go.
    * **Tasks:**
        * In `htmlreport/builder.go`, serialize `model.SummaryResult` (assemblies, classes, overall metrics) to JSON.
        * Embed this JSON as a JavaScript variable (e.g., `window.coverageData`) in the `index.html` generated by Go.
        * Embed necessary settings (e.g., `window.settings`) similarly.
        * Ensure `base_layout.gohtml` contains `<app-root></app-root>`.
        * Go may pre-render parts of the summary table HTML structure that Angular expects, or Angular handles all rendering from JSON.
    * **C# Reference:** `HtmlRenderer.cs` (methods `SummaryAssembly`, `SummaryClass`, `CustomSummary` for data structure).
    * **Status:** PENDING.
* **Step 2.2: Class Detail Pages with Angular Rendering**
    * **Goal:** Generate individual HTML pages for each class, rendered by Angular using class-specific data from Go.
    * **Tasks:**
        * Implement logic for unique class report filenames (e.g., `assemblyName_className.html`).
        * Loop through each `model.Class`, serialize its specific data (files, lines, methods, metrics, history) to JSON for `window.coverageData`.
        * Render an HTML page for each class using `base_layout.gohtml`, embedding the class-specific JSON.
        * Update summary page links to point to these class detail pages.
    * **C# Reference:** `HtmlRenderer.cs` (methods `BeginClassReport`, `CreateClassReport`, `GetClassReportFilename`).
    * **Status:** PENDING.
* **Step 2.3: Source Code Rendering on Detail Pages**
    * **Goal:** Display syntax-highlighted source code on class detail pages, with lines colored by coverage status, using data from Go.
    * **Tasks:**
        * For each `CodeFile` in a class, use `internal/filereader` to read source file content.
        * Combine line content with `model.Line` coverage info (status, branch, hits).
        * Add this per-file line data to `window.coverageData` for the class detail page.
        * Ensure Angular's source view component correctly uses this data and `report.css` provides styling.
    * **C# Reference:** `HtmlRenderer.cs` (line analysis rendering), `LocalFileReader.cs`.
    * **Angular Reference:** `src/app/source-code/source-code.component.ts`
    * **Status:** PENDING.
* **Step 2.4: History Charts**
    * **Goal:** Display coverage history charts on summary and class detail pages, using data from Go.
    * **Tasks:**
        * Serialize `model.SummaryResult.OverallHistoricCoverages` for the summary page and `classModel.HistoricCoverages` for class pages into `window.coverageData`.
        * Implement filtering for history points if necessary.
        * Ensure Angular's history chart component uses this data with Chartist.js.
    * **C# Reference:** `HtmlRenderer.cs` (chart rendering, `FilterHistoricCoverages`).
    * **Angular Reference:** `src/app/history-chart/history-chart.component.ts`
    * **Status:** PENDING.

---
### Phase 3: Advanced Features & Refinements
* **Step 3.1: Inline HTML Option ("HtmlInline" Report Type)**
    * **Goal:** Implement an option to generate self-contained HTML files with all CSS and JS embedded.
    * **Tasks:**
        * Create `HtmlInlineReportBuilder` or add a mode to `HtmlReportBuilder`.
        * Implement logic to read CSS/JS file content and embed within `<style>` and `<script>` tags in the Go templates.
    * **C# Reference:** `HtmlReportBuilderBase.cs` and specific inline builders.
    * **Status:** PENDING.
* **Step 3.2: Risk Hotspots and Other Features**
    * **Goal:** Implement remaining visual features like Risk Hotspots tables, Test Method lists, etc.
    * **Tasks:** (For each feature) Data model check, Go serialization, Angular component verification, Go template updates if needed.
    * **Status:** PENDING.
* **Step 3.3: Theme Support**
    * **Goal:** Allow users to select different visual themes for the HTML report.
    * **Tasks:**
        * Ensure all theme CSS files from C# are available.
        * Implement logic (likely via CLI option) to select a theme.
        * Modify asset copying/CSS combination logic (Step 1.2) to use the selected theme CSS when creating `report.css`.
    * **C# Reference:** `HtmlRenderer.cs` (how themes are handled).
    * **Status:** PENDING.

---
### Phase 4: Testing, Documentation & Finalization
* **Step 4.1: Comprehensive Testing**
    * **Task:** Thoroughly test generated HTML reports. Compare output with C# version using identical Cobertura files. Cross-browser testing.
    * **Status:** PENDING.
* **Step 4.2: Performance Profiling and Optimization**
    * **Task:** Profile Go's report generation process. Identify and address bottlenecks.
    * **Status:** PENDING.
* **Step 4.3: Code Cleanup and Refactoring**
    * **Task:** Review Go code for clarity, efficiency, and adherence to best practices. Refactor as needed.
    * **Status:** PENDING.
* **Step 4.4: User Documentation**
    * **Task:** Update README and any other user documentation to reflect new HTML reporting capabilities, options, and usage.
    * **Status:** PENDING.

---
## 🔑 Key Considerations & Principles
* **Reference C# Project:** The original C# `ReportGenerator` project is the primary source of truth for features, logic, and UI.
* **Expandability:** Design Go interfaces and structures (especially for reporters and data processing) to easily accommodate new report types (e.g., XML, JSON, Markdown Summary) or input formats in the future without major refactoring.
* **Modularity:** Keep components (parsing, analysis, report generation, asset handling) as decoupled as possible.
* **Configuration:** Plan for report generation configuration options (e.g., via CLI, `.json` file).
* **Translations:** The Angular app uses a `translations` JavaScript object. These strings need to be extracted from C#'s `ReportResources.resx` and made available to the Go-generated HTML.

---
## 🔄 Mapping: C# to Go (Assets & Logic)

This section tracks how components from the C# project are handled in the Go port.

| C# Component / Feature                      | Go Port Strategy                                       | Status / Notes                                                                 |
| :------------------------------------------ | :----------------------------------------------------- | :----------------------------------------------------------------------------- |
| **Core Logic & Data Structures** |                                                        |                                                                                |
| `CoberturaParser.cs`                        | Translate to Go (`internal/parser/cobertura.go`)       | DONE (Initial version)                                                         |
| `Parser/Analysis/*` (Data Model)            | Translate/Adapt to Go structs (`internal/model/*`)     | DONE (Initial version, ongoing verification for HTML needs)                    |
| Analyzer logic (populating model)           | Translate/Adapt to Go (`internal/analyzer/*`)          | DONE (Initial version, ongoing verification for HTML needs)                    |
| **HTML Rendering Engine** |                                                        |                                                                                |
| `HtmlRenderer.cs`                           | Translate to Go `html/template` & Go functions       | PENDING (Core of HTML generation logic)                                        |
| `HtmlReportBuilderBase.cs`                  | Adapt concepts for Go `HtmlReportBuilder`              | PENDING                                                                        |
| **Static Assets (Non-Angular)** |                                                        |                                                                                |
| `Reporting/Builders/Rendering/resources/*`  | **Copy** to `go_report_generator/assets/htmlreport/` | PENDING (CSS, JS, themes like `custom_dark.css`)                               |
| **Angular SPA** |                                                        |                                                                                |
| `src/AngularComponents/*`                   | **Copy** to `go_report_generator/angular_frontend_spa/`      | DONE (Entire Angular project)                                               |
| Angular Build Process (`ng build`)          | Replicate via `npm` scripts                            | PENDING                                                                        |
| Angular Data Injection (JS variables)       | Replicate via Go templates embedding JSON              | PENDING                                                                        |
| **Report Resources & Translations** |                                                        |                                                                                |
| `ReportResources.resx` / `*.Designer.cs`    | Extract strings, make available as JS object in Go     | PENDING (For Angular app's i18n)                                               |
| **File Handling** |                                                        |                                                                                |
| `LocalFileReader.cs` / `IFileReader.cs`     | Adapt/Translate to Go (`internal/filereader`)          | PARTIALLY DONE (Basic line counting exists, needs enhancement for content) |
| **Other Report Types (Future)** |                                                        |                                                                                |
| `TextSummaryReportBuilder.cs`               | Translated to Go (`internal/reporter/textsummary`)     | DONE                                                                           |
| Other builders (XML, Badge, Cobertura etc.) | Plan for future translation/adaptation                 | FUTURE                                                                         |

---
## ✅ Completed Milestones Log
* **[Date]** Initial Cobertura XML parsing implemented.
* **[Date]** Basic internal data model created.
* **[Date]** TextSummary report generation functional.
* **[Date]** CLI arguments for input/output and TextSummary report type.
* **[Date]** Phase 0: Pre-flight Checks & Setup - CLI argument for "Html" added.
* **2025-05-29** Phase 1: Basic HTML Structure & Asset Management - Completed all steps including HTML Report Builder setup, static assets integration, and Angular SPA integration.
* **2025-05-29** Phase 1, Step 1.1: HTML Report Builder and Basic Templates (Go) - Initial `HtmlReportBuilder` and `base_layout.gohtml` created.
* **2025-05-29** Phase 1, Step 1.2: Copy and Integrate Static Assets (CSS/JS from C# `resources/`) - Non-Angular CSS/JS assets integrated and linked in HTML templates.
* **2025-05-29** Phase 1, Step 1.3: Integrate Pre-compiled Angular SPA Assets - Angular project copied, build process established, and assets integrated with Go HTML generation.