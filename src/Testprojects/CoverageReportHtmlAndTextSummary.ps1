<#
    Run tests, collect coverage (Cobertura XML),
    then generate an HTML + text summary report -- all in one go.
#>

# ------------------------------------------------------------
# Paths
# ------------------------------------------------------------
$PSScriptRoot     = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$testProjectPath  = Join-Path  $PSScriptRoot "CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj"

$coverageDir      = Join-Path  $PSScriptRoot "CSharp\Reports"
$coverageReport   = Join-Path  $coverageDir   "coverage.cobertura.xml"

$outputPath       = Join-Path  $PSScriptRoot "..\coverage-report"
$reportGenDll     = Join-Path  $PSScriptRoot "..\..\src\ReportGenerator.Console.NetCore\bin\Debug\net8.0\ReportGenerator.dll"

# ------------------------------------------------------------
# Ensure directories exist
# ------------------------------------------------------------
New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null
New-Item -ItemType Directory -Force -Path $outputPath  | Out-Null

# ------------------------------------------------------------
# 1. Run tests + collect coverage
# ------------------------------------------------------------
Write-Host "Running tests and collecting coverage..."
dotnet test $testProjectPath `
    --configuration Release `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="$coverageDir\coverage"

if (-not (Test-Path $coverageReport)) {
    Write-Error "❌  Coverage report was not generated."
    exit 1
}

# ------------------------------------------------------------
# 2. Generate report(s)
# ------------------------------------------------------------
Write-Host "`nGenerating HTML and text summary reports..."
dotnet $reportGenDll `
    "-reports:$coverageReport" `
    "-targetdir:$outputPath" `
    "-reporttypes:TextSummary"

if (Test-Path "$outputPath\Summary.txt") {
    Write-Host "`n✅  Reports generated successfully:"
    Write-Host "   • Text summary : $outputPath\Summary.txt"
    Write-Host "   • HTML report  : $outputPath\index.html"
} else {
    Write-Error "❌  Report generation failed."
    exit 1
}
