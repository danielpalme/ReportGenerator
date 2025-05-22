# PowerShell script to compare test report files between C# and Go project directories

$csReportDir = "c:\www\ReportGenerator\src\Testprojects"
$goReportDir = "c:\www\ReportGenerator\go_report_generator\test\Testprojects"

# Find all XML and info files in both directories
$csFiles = Get-ChildItem -Path $csReportDir -Recurse -Include *.xml,*.info | Where-Object { -not $_.PSIsContainer }
$goFiles = Get-ChildItem -Path $goReportDir -Recurse -Include *.xml,*.info | Where-Object { -not $_.PSIsContainer }

$differences = @()
foreach ($csFile in $csFiles) {
    $relativePath = $csFile.FullName.Substring($csReportDir.Length)
    $goFilePath = Join-Path $goReportDir $relativePath.TrimStart('\')
    if (Test-Path $goFilePath) {
        $csContent = Get-Content $csFile.FullName -Raw
        $goContent = Get-Content $goFilePath -Raw
        if ($csContent -ne $goContent) {
            $differences += $relativePath
        }
    } else {
        $differences += $relativePath + " (missing in Go project)"
    }
}

if ($differences.Count -eq 0) {
    Write-Host "All test report files are identical."
} else {
    Write-Host "Differences found in the following files:"
    $differences | ForEach-Object { Write-Host $_ }
}
