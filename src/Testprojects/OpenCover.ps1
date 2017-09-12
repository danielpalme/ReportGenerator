CoverageTools\OpenCover.4.6.601_beta\tools\OpenCover.Console.exe "-register:user" "-target:CSharp\Project\bin\Debug\Test.exe" "-filter:+[Test]*" "-output:CSharp\Reports\OpenCover.xml" "-excludebyattribute:*.CoverageExclude*"

CoverageTools\OpenCover.4.6.601_beta\tools\OpenCover.Console.exe -register:user -"target:C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe" "-targetargs:/noisolation /testcontainer:CSharp\Project\bin\Debug\Test.exe" "-filter:+[*]*" "-output:CSharp\Reports\OpenCoverWithTrackedMethods.xml" "-coverbytest:*"

CoverageTools\OpenCover.4.6.601_beta\tools\OpenCover.Console.exe -register:user "-target:FSharp\Project\bin\Debug\Test.exe" "-output:FSharp\Reports\OpenCover.xml"

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project")
$replacement = "C:\temp"

(gc "CSharp\Reports\OpenCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\OpenCover.xml" -Encoding UTF8
(gc "CSharp\Reports\OpenCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\OpenCover.xml" -Encoding UTF8

(gc "CSharp\Reports\OpenCoverWithTrackedMethods.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\OpenCoverWithTrackedMethods.xml" -Encoding UTF8
(gc "CSharp\Reports\OpenCoverWithTrackedMethods.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\OpenCoverWithTrackedMethods.xml" -Encoding UTF8

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\FSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\FSharp\Project")

(gc "FSharp\Reports\OpenCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "FSharp\Reports\OpenCover.xml" -Encoding UTF8
(gc "FSharp\Reports\OpenCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "FSharp\Reports\OpenCover.xml" -Encoding UTF8