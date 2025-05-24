import subprocess
import sys
import pathlib

# --- Constants for Paths ---
SCRIPT_ROOT = pathlib.Path(__file__).resolve().parent
TEST_PROJECT_PATH = SCRIPT_ROOT / "CSharp/Project_DotNetCore/UnitTests/UnitTests.csproj"
COVERAGE_OUTPUT_DIR = SCRIPT_ROOT / "CSharp/Reports"
COVERAGE_REPORT_XML_PATH = COVERAGE_OUTPUT_DIR / "coverage.cobertura.xml"
GO_REPORT_GENERATOR_CMD_PATH = SCRIPT_ROOT.parent / "go_report_generator/cmd"
GO_REPORTS_OUTPUT_DIR = SCRIPT_ROOT.parent / "reports/go_generated_report"
DOTNET_REPORT_GENERATOR_DLL_PATH = SCRIPT_ROOT.parent / "src/ReportGenerator.Console.NetCore/bin/Debug/net8.0/ReportGenerator.dll"
DOTNET_REPORTS_OUTPUT_DIR = SCRIPT_ROOT.parent / "reports/dotnet_original_report_generator"
# --- End of Constants ---

def run_command(command_args, working_dir=None, command_name="Command"):
    """Runs a shell command, exits on error, minimal logging for success."""
    cmd_str = ' '.join(map(str, command_args))
    print(f"Executing {command_name}: {cmd_str[:100]}{'...' if len(cmd_str) > 100 else ''}")
    if working_dir:
        print(f"  (in {working_dir})")
    try:
        process = subprocess.run(
            command_args,
            capture_output=True,
            text=True,
            cwd=working_dir,
            check=True, # Raises CalledProcessError on non-zero exit code
            shell=False
        )
        # Optionally print brief success or specific output if needed for a particular command
        # For minimal logging, we often rely on the absence of error messages.
        # if process.stdout.strip():
        # print(f"  Output: {process.stdout.strip()[:100]}") # Example: brief stdout
        return process
    except FileNotFoundError:
        print(f"❌ Error: Command not found - {command_args[0]}. Ensure it's in your PATH.", file=sys.stderr)
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"❌ Error executing {command_name}: {' '.join(map(str, e.cmd))}", file=sys.stderr)
        print(f"  Return code: {e.returncode}", file=sys.stderr)
        if e.stderr:
            print(f"  Stderr:\n{e.stderr.strip()}", file=sys.stderr)
        if e.stdout: # Sometimes errors are also in stdout
            print(f"  Stdout:\n{e.stdout.strip()}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"❌ An unexpected error occurred during {command_name}: {e}", file=sys.stderr)
        sys.exit(1)

def main():
    print("Starting report generation process...")

    # 1. Ensure output directories exist
    for dir_path in [COVERAGE_OUTPUT_DIR, GO_REPORTS_OUTPUT_DIR, DOTNET_REPORTS_OUTPUT_DIR]:
        try:
            dir_path.mkdir(parents=True, exist_ok=True)
        except OSError as e:
            print(f"❌ Error creating directory {dir_path}: {e}", file=sys.stderr)
            sys.exit(1)
    print("Output directories ensured.")

    # 2. Generate Cobertura XML report
    print("\n--- Generating Cobertura XML ---")
    dotnet_test_command = [
        "dotnet", "test", str(TEST_PROJECT_PATH),
        "--configuration", "Release", "--verbosity", "quiet",
        "/p:CollectCoverage=true", "/p:CoverletOutputFormat=cobertura",
        f"/p:CoverletOutput={COVERAGE_REPORT_XML_PATH}"
    ]
    run_command(dotnet_test_command, command_name="dotnet test")
    if not COVERAGE_REPORT_XML_PATH.exists():
        print(f"❌ Error: Cobertura XML not generated at {COVERAGE_REPORT_XML_PATH}", file=sys.stderr)
        sys.exit(1)
    print(f"✅ Cobertura XML generated: {COVERAGE_REPORT_XML_PATH}")

    # 3. Run Go report generator
    print("\n--- Generating report with Go tool ---")
    if not (GO_REPORT_GENERATOR_CMD_PATH / "main.go").exists(): # Minimal check
        print(f"❌ Error: Go 'main.go' not found in {GO_REPORT_GENERATOR_CMD_PATH}", file=sys.stderr)
        sys.exit(1)
    go_report_command = [
        "go", "run", ".",
        "-report", str(COVERAGE_REPORT_XML_PATH),
        "-output", str(GO_REPORTS_OUTPUT_DIR),
        "-reporttypes", "TextSummary"
    ]
    run_command(go_report_command, working_dir=str(GO_REPORT_GENERATOR_CMD_PATH), command_name="Go Report Generator")
    
    go_summary_txt_path = GO_REPORTS_OUTPUT_DIR / "Summary.txt"
    go_report_generated = go_summary_txt_path.exists()
    if go_report_generated:
        print(f"✅ Go TextSummary generated: {go_summary_txt_path}")
    else:
        print(f"❌ Error: Go TextSummary not found at {go_summary_txt_path}", file=sys.stderr)

    # 4. Run .NET ReportGenerator.dll
    print("\n--- Generating report with .NET tool ---")
    if not DOTNET_REPORT_GENERATOR_DLL_PATH.exists():
        print(f"❌ Error: .NET ReportGenerator.dll not found: {DOTNET_REPORT_GENERATOR_DLL_PATH}", file=sys.stderr)
        sys.exit(1)
    
    dotnet_report_types = "TextSummary"
    # dotnet_report_types = "TextSummary;Html" # Uncomment to also generate HTML report

    dotnet_report_generator_command = [
        "dotnet", str(DOTNET_REPORT_GENERATOR_DLL_PATH),
        f"-reports:{COVERAGE_REPORT_XML_PATH}",
        f"-targetdir:{DOTNET_REPORTS_OUTPUT_DIR}",
        f"-reporttypes:{dotnet_report_types}"
    ]
    run_command(dotnet_report_generator_command, command_name=".NET ReportGenerator")

    dotnet_summary_txt_path = DOTNET_REPORTS_OUTPUT_DIR / "Summary.txt"
    dotnet_text_summary_generated = dotnet_summary_txt_path.exists()

    if dotnet_text_summary_generated:
        print(f"✅ .NET TextSummary generated: {dotnet_summary_txt_path}")
    else:
        print(f"❌ Error: .NET TextSummary not found at {dotnet_summary_txt_path}", file=sys.stderr)

    # Optional: Check for HTML if it was requested and intended to be critical
    if "Html" in dotnet_report_types:
        dotnet_index_html_path = DOTNET_REPORTS_OUTPUT_DIR / "index.html"
        if dotnet_index_html_path.exists():
            print(f"  (.NET HTML report also generated: {dotnet_index_html_path})")
        else:
            # This is a warning if TextSummary is the main goal, but could be an error
            print(f"  ⚠️ Warning: .NET HTML report (index.html) was requested but not found.", file=sys.stderr)


    print("\n--- Script Summary ---")
    if go_report_generated and dotnet_text_summary_generated:
        print("✅ All essential TextSummary reports generated successfully.")
    else:
        print("❌ One or more essential TextSummary reports failed to generate. Please review errors.")
        sys.exit(1)
    
    print("Script finished.")

if __name__ == "__main__":
    main()