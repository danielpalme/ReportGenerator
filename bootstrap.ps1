# Bootstrap script for setting up the development environment for ReportGenerator

# Check if .NET SDK is installed
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing .NET SDK..."
    winget install --id Microsoft.DotNet.SDK.9 -e --source winget
} else {
    Write-Host ".NET SDK is already installed."
}

# Verify .NET SDK version
$dotnetVersion = dotnet --version
if ($dotnetVersion -ne "9.0.300") {
    Write-Host "Warning: Expected .NET SDK version 9.0.300, but found $dotnetVersion."
} else {
    Write-Host ".NET SDK version is correct."
}

# Restore and build the solution
Write-Host "Restoring and building the solution..."
cd "c:\www\ReportGenerator\src"
if (-not (dotnet restore ReportGenerator.sln)) {
    Write-Host "Error: Failed to restore the solution."
    exit 1
}
if (-not (dotnet build ReportGenerator.sln)) {
    Write-Host "Error: Failed to build the solution."
    exit 1
}

# Run core tests
Write-Host "Running core tests..."
if (-not (dotnet test ReportGenerator.Core.Test/ReportGenerator.Core.Test.csproj)) {
    Write-Host "Error: Tests failed."
    exit 1
}

# Install ReportGenerator as a global tool
Write-Host "Installing ReportGenerator as a global tool..."
if (-not (Get-Command "reportgenerator" -ErrorAction SilentlyContinue)) {
    dotnet tool install -g dotnet-reportgenerator-globaltool
} else {
    Write-Host "ReportGenerator is already installed."
}

# Verify installation
if (-not (reportgenerator --help)) {
    Write-Host "Error: Failed to verify ReportGenerator installation."
    exit 1
}

Write-Host "Development environment setup completed successfully."
