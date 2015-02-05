& "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe" collect /output:CSharp\Reports\DynamicCodeCoverage.coverage CSharp\Project\bin\Debug\Test.exe
& "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe" analyze /output:CSharp\Reports\DynamicCodeCoverage.xml CSharp\Reports\DynamicCodeCoverage.coverage

& "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe" collect /output:FSharp\Reports\DynamicCodeCoverage.coverage FSharp\Project\bin\Debug\Test.exe
& "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe" analyze /output:FSharp\Reports\DynamicCodeCoverage.xml FSharp\Reports\DynamicCodeCoverage.coverage

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project")
$replacement = "C:\temp"

(gc "CSharp\Reports\DynamicCodeCoverage.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\DynamicCodeCoverage.xml" -Encoding UTF8
(gc "CSharp\Reports\DynamicCodeCoverage.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\DynamicCodeCoverage.xml" -Encoding UTF8

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\FSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\FSharp\Project")

(gc "FSharp\Reports\DynamicCodeCoverage.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "FSharp\Reports\DynamicCodeCoverage.xml" -Encoding UTF8
(gc "FSharp\Reports\DynamicCodeCoverage.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "FSharp\Reports\DynamicCodeCoverage.xml" -Encoding UTF8