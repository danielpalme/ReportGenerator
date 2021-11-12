dotnet test /p:AltCover=true /p:AltCoverXmlReport=..\..\Reports\OpenCover_altcover.xml /p:AltCoverCobertura=..\..\Reports\Cobertura_altcover.xml /p:AltCoverAssemblyFilter=UnitTests CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project_DotNetCore\Test")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project_DotNetCore\UnitTests\bin\Debug\netcoreapp3.1\__Saved")
$replacement = "C:\temp"

(gc "CSharp\Reports\OpenCover_altcover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\OpenCover_altcover.xml" -Encoding UTF8
(gc "CSharp\Reports\OpenCover_altcover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\OpenCover_altcover.xml" -Encoding UTF8

(gc "CSharp\Reports\Cobertura_altcover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\Cobertura_altcover.xml" -Encoding UTF8
(gc "CSharp\Reports\Cobertura_altcover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\Cobertura_altcover.xml" -Encoding UTF8