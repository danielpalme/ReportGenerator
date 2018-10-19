dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=..\..\Reports\OpenCover_coverlet.xml CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\Reports\Cobertura_coverlet.xml CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj

$pathToReplace = [regex]::Escape((Get-Location).Path + "\CSharp\Project_DotNetCore\Test")
$replacement = "C:\temp"

(gc "CSharp\Reports\OpenCover_coverlet.xml") | % { $_ -replace $pathToReplace, $replacement } | Out-File "CSharp\Reports\OpenCover_coverlet.xml" -Encoding UTF8

(gc "CSharp\Reports\Cobertura_coverlet.xml") | % { $_ -replace $pathToReplace, $replacement } | Out-File "CSharp\Reports\Cobertura_coverlet.xml" -Encoding UTF8