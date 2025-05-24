# Get paths
$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$testProjectPath = Join-Path $PSScriptRoot "CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj"
$outputPath = Join-Path $PSScriptRoot "CSharp\Reports"
$goReportPath = Join-Path $PSScriptRoot "..\..\go_report_generator\cmd"

# Ensure output directory exists
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

Write-Host "Running tests and generating Cobertura coverage report..."
Write-Host "Test project: $testProjectPath"
Write-Host "Output directory: $outputPath"

# Run tests with Coverlet to generate Cobertura XML report
dotnet test $testProjectPath `
    --configuration Release `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="$outputPath\coverage.cobertura.xml"

if (-not (Test-Path "$outputPath\coverage.cobertura.xml")) {
    Write-Error "Failed to generate coverage report"
    exit 1
}

Write-Host "`nGenerating report using Go ReportGenerator..."

# Run Go report generator
Set-Location $goReportPath
$outputDir = Join-Path $PSScriptRoot "..\go-coverage-report"
go run . -report "$outputPath\coverage.cobertura.xml" -output $outputDir -reporttypes TextSummary
