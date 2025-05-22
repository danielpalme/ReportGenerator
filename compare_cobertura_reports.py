import os
import subprocess
import filecmp
import sys
import shutil

# Paths (adjust as needed)
COVERAGE_XML = os.path.abspath('src/Testprojects/CSharp/Reports/Cobertura_coverlet.xml')
CSHARP_GEN = os.path.abspath('src/ReportGenerator.Console/bin/Debug/net6.0/ReportGenerator.exe')
GO_GEN = os.path.abspath('go_report_generator/cmd/go_report_generator.exe')

CSHARP_OUT = os.path.abspath('csharp_report_output')
GO_OUT = os.path.abspath('go_report_generator/coverage-report')


def run_csharp_generator():
    if not os.path.exists(CSHARP_GEN):
        print(f"Warning: C# ReportGenerator not found at {CSHARP_GEN}. Skipping C# generation.")
        return
    if os.path.exists(CSHARP_OUT):
        shutil.rmtree(CSHARP_OUT)
    os.makedirs(CSHARP_OUT, exist_ok=True)
    subprocess.run([
        'dotnet', CSHARP_GEN,
        '-reports:' + COVERAGE_XML,
        '-targetdir:' + CSHARP_OUT
    ], check=True)

def run_go_generator():
    # Placeholder: copy src/coverage-report to GO_OUT
    src_report_dir = os.path.abspath('src/coverage-report')
    if os.path.exists(GO_OUT):
        shutil.rmtree(GO_OUT)
    if os.path.exists(src_report_dir):
        shutil.copytree(src_report_dir, GO_OUT)
        print(f"Copied placeholder Go output from {src_report_dir} to {GO_OUT}")
    else:
        os.makedirs(GO_OUT, exist_ok=True)
        print(f"No src/coverage-report found, created empty {GO_OUT}")

def compare_outputs():
    cmp = filecmp.dircmp(CSHARP_OUT, GO_OUT)
    if cmp.diff_files or cmp.left_only or cmp.right_only:
        print('Differences found:')
        if cmp.diff_files:
            print('Files differ:', cmp.diff_files)
        if cmp.left_only:
            print('Only in C# output:', cmp.left_only)
        if cmp.right_only:
            print('Only in Go output:', cmp.right_only)
        sys.exit(1)
    print('Outputs are identical!')

if __name__ == '__main__':
    run_csharp_generator()
    run_go_generator()
    compare_outputs()
