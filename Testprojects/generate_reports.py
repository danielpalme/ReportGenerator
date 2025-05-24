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
GO_PROJECT_TO_TEST_PATH = SCRIPT_ROOT / "Go" # Path to the Go project being tested
GO_PROJECT_NATIVE_COVERAGE_FILE = GO_PROJECT_TO_TEST_PATH / "coverage.out" # Native Go coverage output
GO_PROJECT_COBERTURA_XML_FILE = GO_PROJECT_TO_TEST_PATH / "coverage.cobertura.xml" # Cobertura from Go native
GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR = SCRIPT_ROOT.parent / "reports/go_project_go_tool_report"
GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR = SCRIPT_ROOT.parent / "reports/go_project_dotnet_tool_report"

# --- Common Tool Paths ---
GO_REPORT_GENERATOR_CMD_PATH = SCRIPT_ROOT.parent / "go_report_generator/cmd" # Adjusted based on PS1 for Go
DOTNET_REPORT_GENERATOR_DLL_PATH = SCRIPT_ROOT.parent / "src/ReportGenerator.Console.NetCore/bin/Debug/net8.0/ReportGenerator.dll" # Adjusted based on PS1 for Go
# Note: You might need to adjust GO_REPORT_GENERATOR_CMD_PATH and DOTNET_REPORT_GENERATOR_DLL_PATH
# if their relation to SCRIPT_ROOT (Testprojects/) is different from what's inferred from the PS1 scripts.
# The PS1 `Join-Path $PSScriptRoot "..\..\..."` implies SCRIPT_ROOT.parent.parent

# --- End of Constants ---

def run_command(command_args_or_string, working_dir=None, command_name="Command", shell=False):
    """Runs a shell command, exits on error, minimal logging for success."""
    is_string_command = isinstance(command_args_or_string, str)
    
    if shell and not is_string_command:
        print(f"❌ Error: If shell=True, command must be a string. Got: {command_args_or_string}", file=sys.stderr)
        sys.exit(1)
    if not shell and is_string_command:
        print(f"❌ Error: If shell=False, command must be a list. Got: {command_args_or_string}", file=sys.stderr)
        sys.exit(1)

    cmd_display_str = command_args_or_string if is_string_command else ' '.join(map(str, command_args_or_string))
    print(f"Executing {command_name}: {cmd_display_str[:100]}{'...' if len(cmd_display_str) > 100 else ''}")
    if working_dir:
        print(f"  (in {working_dir})")
    try:
        process = subprocess.run(
            command_args_or_string,
            capture_output=True,
            text=True,
            cwd=working_dir,
            check=True,
            shell=shell
        )
        # Minimal success logging is implicit by not erroring.
        # If specific command outputs are always useful on success, they can be printed here.
        # e.g., if process.stderr.strip(): print(f"  Stderr (success): {process.stderr.strip()}")
        return process
    except FileNotFoundError:
        executable = command_args_or_string if shell else command_args_or_string[0]
        print(f"❌ Error: Command not found - {executable}. Ensure it's in your PATH.", file=sys.stderr)
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"❌ Error executing {command_name}: {e.cmd}", file=sys.stderr)
        print(f"  Return code: {e.returncode}", file=sys.stderr)
        if e.stderr:
            print(f"  Stderr:\n{e.stderr.strip()}", file=sys.stderr)
        if e.stdout:
            print(f"  Stdout:\n{e.stdout.strip()}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"❌ An unexpected error occurred during {command_name}: {e}", file=sys.stderr)
        sys.exit(1)

def ensure_dir(dir_path: pathlib.Path):
    try:
        dir_path.mkdir(parents=True, exist_ok=True)
    except OSError as e:
        print(f"❌ Error creating directory {dir_path}: {e}", file=sys.stderr)
        sys.exit(1)

def check_file_generated(file_path: pathlib.Path, file_description: str):
    if not file_path.exists() or file_path.stat().st_size == 0:
        print(f"❌ Error: {file_description} not generated or is empty at {file_path}", file=sys.stderr)
        return False
    print(f"✅ {file_description} generated: {file_path}")
    return True

def run_csharp_workflow():
    print("\n--- Starting C# Project Workflow ---")

    # 1. Ensure output directories
    for dir_path in [CSHARP_COVERAGE_OUTPUT_DIR, CSHARP_REPORTS_FROM_GO_TOOL_DIR, CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR]:
        ensure_dir(dir_path)
    print("C# workflow output directories ensured.")

    # 2. Generate Cobertura XML for C#
    print("\n--- Generating Cobertura XML for C# project ---")
    dotnet_test_command = [
        "dotnet", "test", str(CSHARP_TEST_PROJECT_PATH),
        "--configuration", "Release", "--verbosity", "minimal",
        "/p:CollectCoverage=true", "/p:CoverletOutputFormat=cobertura",
        f"/p:CoverletOutput={CSHARP_COBERTURA_XML_PATH}"
    ]
    run_command(dotnet_test_command, command_name="dotnet test (C#)")
    if not check_file_generated(CSHARP_COBERTURA_XML_PATH, "C# Cobertura XML"): sys.exit(1)

    # 3. Run Go report generator for C# Cobertura
    print("\n--- Generating report with Go tool for C# Cobertura ---")
    go_report_command_csharp = [
        "go", "run", ".",
        "-report", str(CSHARP_COBERTURA_XML_PATH),
        "-output", str(CSHARP_REPORTS_FROM_GO_TOOL_DIR),
        "-reporttypes", "TextSummary"
    ]
    run_command(go_report_command_csharp, working_dir=str(GO_REPORT_GENERATOR_CMD_PATH), command_name="Go Report Generator (for C#)")
    csharp_go_summary_ok = check_file_generated(CSHARP_REPORTS_FROM_GO_TOOL_DIR / "Summary.txt", "C# Go-tool TextSummary")

    # 4. Run .NET ReportGenerator for C# Cobertura
    print("\n--- Generating report with .NET tool for C# Cobertura ---")
    dotnet_report_types = "TextSummary"
    # dotnet_report_types = "TextSummary;Html" # Uncomment for HTML
    dotnet_rg_command_csharp = [
        "dotnet", str(DOTNET_REPORT_GENERATOR_DLL_PATH),
        f"-reports:{CSHARP_COBERTURA_XML_PATH}",
        f"-targetdir:{CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR}",
        f"-reporttypes:{dotnet_report_types}"
    ]
    run_command(dotnet_rg_command_csharp, command_name=".NET ReportGenerator (for C#)")
    csharp_dotnet_summary_ok = check_file_generated(CSHARP_REPORTS_FROM_DOTNET_TOOL_DIR / "Summary.txt", "C# .NET-tool TextSummary")

    if not (csharp_go_summary_ok and csharp_dotnet_summary_ok):
        print("❌ C# workflow: One or more TextSummary reports failed to generate.", file=sys.stderr)
        sys.exit(1)
    print("--- C# Project Workflow Finished Successfully ---")


def run_go_project_workflow():
    print("\n--- Starting Go Project Workflow ---")

    # 1. Ensure output directories
    for dir_path in [GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR, GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR]:
        ensure_dir(dir_path)
    if not GO_PROJECT_TO_TEST_PATH.exists():
        print(f"❌ Error: Go project to test not found at {GO_PROJECT_TO_TEST_PATH}", file=sys.stderr)
        sys.exit(1)
    print("Go workflow output directories ensured.")

    # 2. Clean up old Go coverage files
    GO_PROJECT_NATIVE_COVERAGE_FILE.unlink(missing_ok=True)
    GO_PROJECT_COBERTURA_XML_FILE.unlink(missing_ok=True)
    print("Old Go coverage files removed.")

    # 3. Run Go tests & collect native coverage
    print("\n--- Generating native Go coverage ---")
    go_test_command = [
        "go", "test", f"-coverprofile={GO_PROJECT_NATIVE_COVERAGE_FILE.name}", "./..."
        # .name is used because -coverprofile path is relative to working_dir
    ]
    run_command(go_test_command, working_dir=str(GO_PROJECT_TO_TEST_PATH), command_name="go test (Go project)")
    if not check_file_generated(GO_PROJECT_NATIVE_COVERAGE_FILE, "Go native coverage"): sys.exit(1)

    # 4. Convert Go native coverage to Cobertura XML
    print("\n--- Converting Go native coverage to Cobertura XML ---")
    # Using shell=True here to handle redirection as in the PowerShell script (cmd /c ...)
    # Ensure gocover-cobertura.exe is in PATH
    gocover_cobertura_command_str = (
        f"gocover-cobertura.exe < \"{GO_PROJECT_NATIVE_COVERAGE_FILE.name}\""
        f" > \"{GO_PROJECT_COBERTURA_XML_FILE.name}\""
    )
    run_command(gocover_cobertura_command_str,
                working_dir=str(GO_PROJECT_TO_TEST_PATH), # Must run where the files are
                command_name="gocover-cobertura",
                shell=True)
    if not check_file_generated(GO_PROJECT_COBERTURA_XML_FILE, "Go project Cobertura XML"): sys.exit(1)

    # 5. Run Go report generator for Go project's Cobertura
    print("\n--- Generating report with Go tool for Go Cobertura ---")
    go_report_command_go_proj = [
        "go", "run", ".",
        "-report", str(GO_PROJECT_COBERTURA_XML_FILE),
        "-output", str(GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR),
        "-reporttypes", "TextSummary"
    ]
    run_command(go_report_command_go_proj, working_dir=str(GO_REPORT_GENERATOR_CMD_PATH), command_name="Go Report Generator (for Go project)")
    go_proj_go_summary_ok = check_file_generated(GO_PROJECT_REPORTS_FROM_GO_TOOL_DIR / "Summary.txt", "Go-project Go-tool TextSummary")

    # 6. Run .NET ReportGenerator for Go project's Cobertura
    print("\n--- Generating report with .NET tool for Go Cobertura ---")
    dotnet_report_types_go_proj = "TextSummary"
    # dotnet_report_types_go_proj = "TextSummary;Html" # Uncomment for HTML
    dotnet_rg_command_go_proj = [
        "dotnet", str(DOTNET_REPORT_GENERATOR_DLL_PATH),
        f"-reports:{GO_PROJECT_COBERTURA_XML_FILE}",
        f"-targetdir:{GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR}",
        f"-reporttypes:{dotnet_report_types_go_proj}"
    ]
    run_command(dotnet_rg_command_go_proj, command_name=".NET ReportGenerator (for Go project)")
    go_proj_dotnet_summary_ok = check_file_generated(GO_PROJECT_REPORTS_FROM_DOTNET_TOOL_DIR / "Summary.txt", "Go-project .NET-tool TextSummary")
    
    if not (go_proj_go_summary_ok and go_proj_dotnet_summary_ok):
        print("❌ Go project workflow: One or more TextSummary reports failed to generate.", file=sys.stderr)
        sys.exit(1)
    print("--- Go Project Workflow Finished Successfully ---")


def main():
    print("Python script for C# and Go project coverage and reporting.")

    # Verify common tool paths early
    if not GO_REPORT_GENERATOR_CMD_PATH.exists() or not (GO_REPORT_GENERATOR_CMD_PATH / "main.go").exists():
         print(f"❌ Error: Go Report Generator 'main.go' not found in {GO_REPORT_GENERATOR_CMD_PATH}", file=sys.stderr)
         sys.exit(1)
    if not DOTNET_REPORT_GENERATOR_DLL_PATH.exists():
        print(f"❌ Error: .NET ReportGenerator.dll not found: {DOTNET_REPORT_GENERATOR_DLL_PATH}", file=sys.stderr)
        sys.exit(1)
    print("Common report generation tools found.")

    run_csharp_workflow()
    run_go_project_workflow()

    print("\nAll workflows completed successfully.")

if __name__ == "__main__":
    main()