# TODO: Before running this script to generate Go reports,
# ensure the Angular SPA is built. Navigate to
# 'go_report_generator/angular_frontend_spa/' and run:
# npm install (if not done already)
# npm run build
#!/usr/bin/env python3
"""
Universal Coverage Reporter Script

This script automates the process of generating code coverage reports for both C# (.NET)
and Go projects. It performs the following main steps:

1.  **For C# Projects**:
    * Runs `dotnet test` to generate Cobertura XML coverage files.
    * Uses a custom Go-based report generator (`go_report_generator`) to create reports
        from the Cobertura XML.
    * Uses the standard .NET `ReportGenerator` tool to create reports from the
        Cobertura XML.

2.  **For Go Projects**:
    * Runs `go test` to generate native Go coverage profiles.
    * Converts the native Go coverage to Cobertura XML format using `gocover-cobertura`.
    * Uses the custom Go-based report generator (`go_report_generator`) to create
        reports from the Cobertura XML.
    * Uses the standard .NET `ReportGenerator` tool to create reports from the
        Cobertura XML.

The script allows specifying desired report types by modifying the
`SELECTED_REPORT_TYPES_CONFIG_STRING` variable in the `main` function.

Prerequisites:
    * .NET SDK (for `dotnet test` and `ReportGenerator.dll`)
    * Go (for `go test`, `go run`, and `gocover-cobertura`)
    * `gocover-cobertura` executable in your PATH or accessible.
        (Can be installed via `go install github.com/t-yuki/gocover-cobertura@latest`)
    * The custom `go_report_generator` project correctly set up.
    * The .NET `ReportGenerator.Console.NetCore.dll` built and accessible.

Directory Structure Assumption:
    The script assumes a specific directory structure:
    <root>/
    ├── Testprojects/ (This script is located here, e.g., Testprojects/run_coverage.py)
    │   ├── CSharp/
    │   │   ├── Project_DotNetCore/
    │   │   │   └── UnitTests/UnitTests.csproj
    │   │   └── Reports/ (Output for C# Cobertura and generated reports)
    │   └── Go/ (Go project to be tested, also output for Go coverage files)
    ├── reports/ (Parent directory for consolidated reports)
    │   ├── csharp_project_go_tool_report/
    │   ├── csharp_project_dotnet_tool_report/
    │   ├── go_project_go_tool_report/
    │   ├── go_project_dotnet_tool_report/
    ├── go_report_generator/
    │   └── cmd/main.go (Custom Go report generator tool)
    └── src/
        └── ReportGenerator.Console.NetCore/
            └── bin/Debug/net8.0/ReportGenerator.dll (.NET ReportGenerator tool)

Usage Example:
    To run the script:
    1. Edit the `SELECTED_REPORT_TYPES_CONFIG_STRING` in the `main` function
       of this script to define your desired comma-separated report types
       (e.g., "TextSummary,Html").
    2. Run the script from the `Testprojects` directory:
       ```bash
       python run_coverage.py
       ```
"""
import subprocess
import sys
import pathlib
import os

# --- Constants for Paths ---
SCRIPT_ROOT = pathlib.Path(__file__).resolve().parent

# --- C# Project Specific Paths ---
CSHARP_TEST_PROJECT_PATH = SCRIPT_ROOT / "CSharp/Project_DotNetCore/UnitTests/UnitTests.csproj"
CSHARP_COVERAGE_OUTPUT_DIR = SCRIPT_ROOT / "CSharp/Reports"
CSHARP_COBERTURA_XML_PATH = CSHARP_COVERAGE_OUTPUT_DIR / "coverage.cobertura.xml"
CSHARP_REPORTS_FROM_GO_TOOL_DIR = SCRIPT_ROOT.parent / "reports/csharp_project_go_tool_report"
CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR = SCRIPT_ROOT.parent / "reports/csharp_project_dotnet_tool_report"

# --- Go Project Specific Paths ---
GO_PROJECT_TO_TEST_PATH = SCRIPT_ROOT / "Go"  # Path to the Go project being tested
GO_PROJECT_NATIVE_COVERAGE_FILE = GO_PROJECT_TO_TEST_PATH / "coverage.out"  # Native Go coverage output
GO_PROJECT_COBERTURA_XML_FILE = GO_PROJECT_TO_TEST_PATH / "coverage.cobertura.xml"  # Cobertura from Go native
GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR = SCRIPT_ROOT.parent / "reports/go_project_go_tool_report"
GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR = SCRIPT_ROOT.parent / "reports/go_project_dotnet_tool_report"

# --- Common Tool Paths ---
# Assuming go_report_generator is at the root of your project, sibling to Testprojects
GO_REPORT_GENERATOR_CMD_PATH = SCRIPT_ROOT.parent / "go_report_generator/cmd"
# Assuming src is at the root, sibling to Testprojects
DOTNET_REPORT_GENERATOR_DLL_PATH = SCRIPT_ROOT.parent / "src/ReportGenerator.Console.NetCore/bin/Debug/net8.0/ReportGenerator.dll"

# --- Report File Names (for verification) ---
TEXT_SUMMARY_FILE_NAME = "Summary.txt"
HTML_REPORT_INDEX_FILE_NAME = "index.html" # Common for .NET ReportGenerator HTML reports

# --- End of Constants ---

def run_command(command_args_or_string, working_dir=None, command_name="Command", shell=False):
    """Runs a shell command, prints output, and exits on error."""
    is_string_command = isinstance(command_args_or_string, str)

    if shell and not is_string_command:
        print(f"Error: If shell=True, command must be a string. Got: {command_args_or_string}", file=sys.stderr)
        sys.exit(1)
    if not shell and is_string_command:
        # For flexibility, allow string if shell=False, but split it using shlex
        # However, the script mostly uses lists for shell=False, which is preferred.
        # This check can be made stricter if needed.
        # For now, let's assume the user will pass a list for shell=False.
        # This part of the check can be refined based on actual usage patterns.
        pass # Original script had an error here, but subprocess.run can take a string if it's just the exe.

    cmd_display_str = command_args_or_string if is_string_command else ' '.join(map(str, command_args_or_string))
    print(f"Executing {command_name}: {cmd_display_str[:120]}{'...' if len(cmd_display_str) > 120 else ''}")
    if working_dir:
        print(f"  (in {working_dir})")
    try:
        process = subprocess.run(
            command_args_or_string,
            capture_output=True,
            text=True,
            cwd=working_dir,
            check=False, # We will check returncode manually for better error reporting
            shell=shell,
            env=os.environ.copy() # Pass current environment
        )

        if process.stdout:
            print(f"  Stdout from {command_name}:\n{process.stdout.strip()}")
        if process.stderr:
            print(f"  Stderr from {command_name}:\n{process.stderr.strip()}", file=sys.stderr)

        if process.returncode != 0:
            print(f"Error executing {command_name} (Return code: {process.returncode})", file=sys.stderr)
            sys.exit(1)

        return process
    except FileNotFoundError:
        executable = command_args_or_string if shell else command_args_or_string[0]
        print(f"Error: Command not found - {executable}. Ensure it's in your PATH or correctly specified.", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"An unexpected error occurred during {command_name}: {e}", file=sys.stderr)
        sys.exit(1)

def ensure_dir(dir_path: pathlib.Path):
    """Creates a directory if it doesn't exist."""
    try:
        dir_path.mkdir(parents=True, exist_ok=True)
        print(f"Directory ensured: {dir_path}")
    except OSError as e:
        print(f"Error creating directory {dir_path}: {e}", file=sys.stderr)
        sys.exit(1)

def check_generated_files(output_dir: pathlib.Path, report_types: list[str], tool_name: str) -> bool:
    """
    Checks if expected report files were generated based on report_types.
    Returns True if all expected files are found and non-empty, False otherwise.
    """
    all_ok = True
    checked_something = False

    print(f"Verifying generated reports in {output_dir} for {tool_name} with types: {', '.join(report_types)}...")

    if not output_dir.is_dir():
        print(f"Error: Output directory {output_dir} for {tool_name} does not exist.", file=sys.stderr)
        return False

    if "TextSummary" in report_types:
        checked_something = True
        summary_file = output_dir / TEXT_SUMMARY_FILE_NAME
        if not summary_file.exists() or summary_file.stat().st_size == 0:
            print(f"Error: {tool_name} TextSummary report not generated or is empty at {summary_file}", file=sys.stderr)
            all_ok = False
        else:
            print(f"{tool_name} TextSummary report generated: {summary_file}")

    html_report_types_keywords = {"Html", "HtmlInline", "HtmlChart", "HtmlSummary", "Html_Dark"}
    if any(rt_keyword in rt for rt_keyword in html_report_types_keywords for rt in report_types):
        checked_something = True
        index_html_file = output_dir / HTML_REPORT_INDEX_FILE_NAME
        if not index_html_file.exists() or index_html_file.stat().st_size == 0:
            print(f"Error: {tool_name} HTML report (index.html) not generated or is empty at {index_html_file}", file=sys.stderr)
            all_ok = False
        else:
            print(f"{tool_name} HTML report (index.html) generated: {index_html_file}")
    
    # Example for a generic "Xml" report type check
    if "Xml" in report_types:
        checked_something = True
        # .NET ReportGenerator often names this "Coverage.xml" or similar in the root of targetdir
        # The custom Go tool might name it differently or put it in a subfolder.
        # This check might need refinement based on actual output of your tools for "Xml" type.
        potential_xml_files = list(output_dir.glob("*.xml")) # Basic check
        # A more specific check if the name is known:
        # xml_file = output_dir / "Coverage.xml" # Or "Cobertura.xml" etc.
        # if not xml_file.exists() or xml_file.stat().st_size == 0:

        if not potential_xml_files or not any(f.stat().st_size > 0 for f in potential_xml_files):
            print(f"Error: {tool_name} XML report not found or all potential XML files are empty in {output_dir}", file=sys.stderr)
            all_ok = False
        else:
            print(f"{tool_name} XML report (or similar) likely generated in: {output_dir}")


    if not checked_something and report_types:
        print(f"Warning: No specific file checks implemented for configured report types: {', '.join(report_types)} by {tool_name}. Assuming success if command ran.", file=sys.stderr)
        return True # Default to true if command ran and no specific checks failed.

    return all_ok


def run_csharp_workflow(report_types_list: list[str]):
    print("\n--- Starting C# Project Workflow ---")
    for dir_path in [CSHARP_COVERAGE_OUTPUT_DIR, CSHARP_REPORTS_FROM_GO_TOOL_DIR, CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR]:
        ensure_dir(dir_path)

    print("\n--- Generating Cobertura XML for C# project ---")
    dotnet_test_command = [
        "dotnet", "test", str(CSHARP_TEST_PROJECT_PATH),
        "--configuration", "Release", "--verbosity", "minimal",
        "/p:CollectCoverage=true", "/p:CoverletOutputFormat=cobertura",
        f"/p:CoverletOutput={CSHARP_COBERTURA_XML_PATH.resolve()}"
    ]
    run_command(dotnet_test_command, command_name="dotnet test (C#)")
    if not (CSHARP_COBERTURA_XML_PATH.exists() and CSHARP_COBERTURA_XML_PATH.stat().st_size > 0):
        print(f"Error: C# Cobertura XML not generated or is empty at {CSHARP_COBERTURA_XML_PATH}", file=sys.stderr)
        sys.exit(1)
    print(f"C# Cobertura XML generated: {CSHARP_COBERTURA_XML_PATH}")


    # --- Report Generation with Go tool ---
    if GO_REPORT_GENERATOR_CMD_PATH.is_dir() and (GO_REPORT_GENERATOR_CMD_PATH / "main.go").is_file():
        print("\n--- Generating C# report with Go tool ---")
        go_tool_report_types_arg_value = ",".join(report_types_list)
        go_report_command_csharp = [
            "go", "run", ".",
            f"-report={CSHARP_COBERTURA_XML_PATH.resolve()}",
            f"-output={CSHARP_REPORTS_FROM_GO_TOOL_DIR.resolve()}",
            f"-reporttypes={go_tool_report_types_arg_value}" # Assumes Go tool uses -reporttypes=type1,type2
        ]
        run_command(go_report_command_csharp, working_dir=GO_REPORT_GENERATOR_CMD_PATH, command_name="Go Report Generator (for C#)")
        if not check_generated_files(CSHARP_REPORTS_FROM_GO_TOOL_DIR, report_types_list, "C# Go-tool"):
            print("Error C# workflow: Go tool report generation verification failed.", file=sys.stderr)
            sys.exit(1)
    else:
        print("Skipping Go Report Generator for C#: Tool not found or not configured.")


    # --- Report Generation with .NET tool ---
    if DOTNET_REPORT_GENERATOR_DLL_PATH.exists():
        print("\n--- Generating C# report with .NET tool ---")
        dotnet_tool_report_types_arg_value = ";".join(report_types_list) # .NET ReportGenerator uses semicolon
        dotnet_rg_command_csharp = [
            "dotnet", str(DOTNET_REPORT_GENERATOR_DLL_PATH.resolve()),
            f"-reports:{CSHARP_COBERTURA_XML_PATH.resolve()}",
            f"-targetdir:{CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR.resolve()}",
            f"-reporttypes:{dotnet_tool_report_types_arg_value}"
        ]
        run_command(dotnet_rg_command_csharp, command_name=".NET ReportGenerator (for C#)")
        if not check_generated_files(CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR, report_types_list, "C# .NET-tool"):
            print("Error C# workflow: .NET tool report generation verification failed.", file=sys.stderr)
            sys.exit(1)
    else:
        print("Skipping .NET ReportGenerator for C#: Tool not found or not configured.")

    print("--- C# Project Workflow Finished Successfully ---")


def run_go_project_workflow(report_types_list: list[str]):
    print("\n--- Starting Go Project Workflow ---")
    for dir_path in [GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR, GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR]:
        ensure_dir(dir_path)

    if not GO_PROJECT_TO_TEST_PATH.is_dir():
        print(f"Error: Go project to test not found or is not a directory at {GO_PROJECT_TO_TEST_PATH}", file=sys.stderr)
        sys.exit(1)
    print(f"Go project directory found: {GO_PROJECT_TO_TEST_PATH}")

    GO_PROJECT_NATIVE_COVERAGE_FILE.unlink(missing_ok=True)
    GO_PROJECT_COBERTURA_XML_FILE.unlink(missing_ok=True)
    print("Old Go coverage files removed.")

    print("\n--- Generating native Go coverage ---")
    go_test_command = [
        "go", "test", f"-coverprofile={GO_PROJECT_NATIVE_COVERAGE_FILE.name}", "./..."
    ]
    run_command(go_test_command, working_dir=GO_PROJECT_TO_TEST_PATH, command_name="go test (Go project)")
    if not (GO_PROJECT_NATIVE_COVERAGE_FILE.exists() and GO_PROJECT_NATIVE_COVERAGE_FILE.stat().st_size > 0):
        print(f"Error: Go native coverage not generated or is empty at {GO_PROJECT_NATIVE_COVERAGE_FILE}", file=sys.stderr)
        sys.exit(1)
    print(f"Go native coverage generated: {GO_PROJECT_NATIVE_COVERAGE_FILE}")

    print("\n--- Converting Go native coverage to Cobertura XML ---")
    gocover_cobertura_command_str = (
        f"gocover-cobertura < \"{GO_PROJECT_NATIVE_COVERAGE_FILE.name}\""
        f" > \"{GO_PROJECT_COBERTURA_XML_FILE.name}\""
    )
    run_command(gocover_cobertura_command_str,
                working_dir=GO_PROJECT_TO_TEST_PATH,
                command_name="gocover-cobertura",
                shell=True)
    if not (GO_PROJECT_COBERTURA_XML_FILE.exists() and GO_PROJECT_COBERTURA_XML_FILE.stat().st_size > 0):
        print(f"Error: Go project Cobertura XML not generated or is empty at {GO_PROJECT_COBERTURA_XML_FILE}", file=sys.stderr)
        sys.exit(1)
    print(f"Go project Cobertura XML generated: {GO_PROJECT_COBERTURA_XML_FILE}")

    # --- Report Generation with Go tool ---
    if GO_REPORT_GENERATOR_CMD_PATH.is_dir() and (GO_REPORT_GENERATOR_CMD_PATH / "main.go").is_file():
        print("\n--- Generating Go project report with Go tool ---")
        go_tool_report_types_arg_value = ",".join(report_types_list)
        go_report_command_go_proj = [
            "go", "run", ".",
            f"-report={GO_PROJECT_COBERTURA_XML_FILE.resolve()}",
            f"-output={GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR.resolve()}",
            f"-reporttypes={go_tool_report_types_arg_value}"
        ]
        run_command(go_report_command_go_proj, working_dir=GO_REPORT_GENERATOR_CMD_PATH, command_name="Go Report Generator (for Go project)")
        if not check_generated_files(GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR, report_types_list, "Go-project Go-tool"):
            print("Error Go project workflow: Go tool report generation verification failed.", file=sys.stderr)
            sys.exit(1)
    else:
        print("Skipping Go Report Generator for Go Project: Tool not found or not configured.")


    # --- Report Generation with .NET tool ---
    if DOTNET_REPORT_GENERATOR_DLL_PATH.exists():
        print("\n--- Generating Go project report with .NET tool ---")
        dotnet_tool_report_types_arg_value = ";".join(report_types_list)
        dotnet_rg_command_go_proj = [
            "dotnet", str(DOTNET_REPORT_GENERATOR_DLL_PATH.resolve()),
            f"-reports:{GO_PROJECT_COBERTURA_XML_FILE.resolve()}",
            f"-targetdir:{GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR.resolve()}",
            f"-reporttypes:{dotnet_tool_report_types_arg_value}"
        ]
        run_command(dotnet_rg_command_go_proj, command_name=".NET ReportGenerator (for Go project)")
        if not check_generated_files(GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR, report_types_list, "Go-project .NET-tool"):
            print("Error Go project workflow: .NET tool report generation verification failed.", file=sys.stderr)
            sys.exit(1)
    else:
        print("Skipping .NET ReportGenerator for Go Project: Tool not found or not configured.")

    print("--- Go Project Workflow Finished Successfully ---")

def main():
    """Main function to orchestrate C# and Go project coverage and reporting."""
    print("Python script for C# and Go project coverage and reporting.")

    # --- Define desired report types here ---
    # Uncomment one of the following lines to set the desired report types.
    # Use a comma-separated string for multiple types.
    # Examples: "TextSummary", "Html", "Xml", "Html,TextSummary"
    # Ensure your report generator tools support the types you specify.
    #
    # SELECTED_REPORT_TYPES_CONFIG_STRING = "TextSummary"
    SELECTED_REPORT_TYPES_CONFIG_STRING = "Html"
    # SELECTED_REPORT_TYPES_CONFIG_STRING = "Html,TextSummary"
    # SELECTED_REPORT_TYPES_CONFIG_STRING = "Html,TextSummary,Xml" # Ensure 'Xml' is handled by check_generated_files and tools
    # SELECTED_REPORT_TYPES_CONFIG_STRING = "Cobertura" # If you want Cobertura output from ReportGenerator
    # SELECTED_REPORT_TYPES_CONFIG_STRING = "" # This would cause an error

    if not SELECTED_REPORT_TYPES_CONFIG_STRING or SELECTED_REPORT_TYPES_CONFIG_STRING.isspace():
        print("Error: SELECTED_REPORT_TYPES_CONFIG_STRING is empty. Please define at least one report type.", file=sys.stderr)
        sys.exit(1)

    # Parse the string into a list of report types
    active_report_types = [rt.strip() for rt in SELECTED_REPORT_TYPES_CONFIG_STRING.split(',') if rt.strip()]

    if not active_report_types:
        print(f"Error: No valid report types parsed from '{SELECTED_REPORT_TYPES_CONFIG_STRING}'. Please check the format.", file=sys.stderr)
        sys.exit(1)

    print(f"Target report types: {', '.join(active_report_types)}")


    # --- Pre-flight checks for tools ---
    go_main_file = GO_REPORT_GENERATOR_CMD_PATH / "main.go"
    if not GO_REPORT_GENERATOR_CMD_PATH.is_dir() or not go_main_file.is_file():
        print(f"Warning: Go Report Generator 'main.go' not found in {GO_REPORT_GENERATOR_CMD_PATH} or path is not a directory. Go tool reporting will be skipped.", file=sys.stderr)
    else:
        print(f"Go Report Generator found at: {GO_REPORT_GENERATOR_CMD_PATH}")

    if not DOTNET_REPORT_GENERATOR_DLL_PATH.exists():
        print(f"Warning: .NET ReportGenerator.dll not found: {DOTNET_REPORT_GENERATOR_DLL_PATH}. .NET tool reporting will be skipped.", file=sys.stderr)
    else:
        print(f".NET ReportGenerator.dll found at: {DOTNET_REPORT_GENERATOR_DLL_PATH}")

    # --- Execute Workflows ---
    try:
        run_csharp_workflow(report_types_list=active_report_types)
        run_go_project_workflow(report_types_list=active_report_types)
        print("\nAll workflows completed successfully. Reports generated!")
    except SystemExit as e:
        print(f"\nScript terminated prematurely with exit code {e.code}.", file=sys.stderr)
    except Exception as e:
        print(f"\nAn unexpected error occurred in main: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()