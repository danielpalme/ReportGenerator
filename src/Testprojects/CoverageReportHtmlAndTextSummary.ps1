# Get paths
$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$reportPath = Join-Path $PSScriptRoot "CSharp\Reports\coverage.cobertura.xml"
$outputPath = Join-Path $PSScriptRoot "..\coverage-report"

# Ensure output directory exists
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

Write-Host "Generating HTML report..."
Write-Host "Source XML: $reportPath"
Write-Host "Output directory: $outputPath"

# Find ReportGenerator executable - look for it in the build output directory
$reportGenPath = Join-Path $PSScriptRoot "..\..\src\ReportGenerator.Console.NetCore\bin\Debug\net8.0\ReportGenerator.dll"

# Generate HTML report and text summary using ReportGenerator
dotnet $reportGenPath `
    "-reports:$reportPath" `
    "-targetdir:$outputPath" `
    "-reporttypes:TextSummary"

if (Test-Path "$outputPath\Summary.txt") {
    Write-Host "Text summary report generated successfully at: $outputPath\Summary.txt"
} else {
    Write-Error "Failed to generate HTML report"
    exit 1
}
