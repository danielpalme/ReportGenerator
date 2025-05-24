# Get test project path
$testProjectPath = Join-Path $PSScriptRoot "CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj"
$outputPath = Join-Path $PSScriptRoot "CSharp\Reports"

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

if (Test-Path "$outputPath\coverage.cobertura.xml") {
    Write-Host "Coverage report generated successfully at: $outputPath\coverage.cobertura.xml"
} else {
    Write-Error "Failed to generate coverage report"
    exit 1
}
