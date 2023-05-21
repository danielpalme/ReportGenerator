param(
[string]$From,
[string]$To)

Write-Host $From
Write-Host $To

((Get-Content -path build.proj -Raw) -replace $From, $To) | Set-Content -Path build.proj -NoNewline
((Get-Content -path ..\.github\workflows\ci.yml -Raw) -replace $From, $To) | Set-Content -Path ..\.github\workflows\ci.yml -NoNewline
(Get-ChildItem -Recurse -Filter ReportGenerator*.csproj | Select-String $From) | ForEach-Object { ((Get-Content -path $_.Path -Raw) -replace $From, $To) | Out-File $_.Path -Encoding UTF8 -NoNewline }
((Get-Content -path AzureDevopsTask\vss-extension.json -Raw) -replace $From, $To) | Set-Content -Path AzureDevopsTask\vss-extension.json -NoNewline
((Get-Content -path Deployment\nuget\Readme_dotnet-reportgenerator-globaltool.md -Raw) -replace $From, $To) | Set-Content -Path Deployment\nuget\Readme_dotnet-reportgenerator-globaltool.md -NoNewline
((Get-Content -path Deployment\nuget\Readme_ReportGenerator.Core.md -Raw) -replace $From, $To) | Set-Content -Path Deployment\nuget\Readme_ReportGenerator.Core.md -NoNewline
((Get-Content -path Deployment\nuget\Readme_ReportGenerator.md -Raw) -replace $From, $To) | Set-Content -Path Deployment\nuget\Readme_ReportGenerator.md -NoNewline

$FromVersions = $From.Split(".")
$ToVersions = $To.Split(".")

((((Get-Content -path AzureDevopsTask\ReportGenerator\task.json -Raw) -replace ("""Major"": " + $FromVersions[0]), ("""Major"": " + $ToVersions[0])) -replace ("""Minor"": " + $FromVersions[1]), ("""Minor"": " + $ToVersions[1])) -replace ("""Patch"": " + $FromVersions[2]), ("""Patch"": " + $ToVersions[2])) | Set-Content -Path AzureDevopsTask\ReportGenerator\task.json -NoNewline