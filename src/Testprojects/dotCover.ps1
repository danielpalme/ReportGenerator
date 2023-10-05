CoverageTools\jetbrains.dotcover.commandlinetools.2023.3.0-tc03\dotCover.exe dotnet --reportType=DetailedXML --output=CSharp\Reports\dotCover.xml -- test CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj

CoverageTools\jetbrains.dotcover.commandlinetools.2023.3.0-tc03\dotCover.exe  a /TargetExecutable=FSharp\Project\bin\Debug\Test.exe /ReportType=DetailedXML /Output=FSharp\Reports\dotCover.xml

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project_DotNetCore\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project_DotNetCore")
$replacement = "C:\temp"

(gc "CSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\dotCover.xml" -Encoding UTF8
(gc "CSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\dotCover.xml" -Encoding UTF8

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\FSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\FSharp\Project")

(gc "FSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "FSharp\Reports\dotCover.xml" -Encoding UTF8
(gc "FSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "FSharp\Reports\dotCover.xml" -Encoding UTF8