..\packages\JetBrains.dotCover.CommandLineTools.2016.2.20160818.172304\tools\dotCover.exe a /TargetExecutable=CSharp\Project\bin\Debug\Test.exe /ReportType=DetailedXML /Output=CSharp\Reports\dotCover.xml

..\packages\JetBrains.dotCover.CommandLineTools.2016.2.20160818.172304\tools\dotCover.exe  a /TargetExecutable=FSharp\Project\bin\Debug\Test.exe /ReportType=DetailedXML /Output=FSharp\Reports\dotCover.xml

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project")
$replacement = "C:\temp"

(gc "CSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\dotCover.xml" -Encoding UTF8
(gc "CSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\dotCover.xml" -Encoding UTF8

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\FSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\FSharp\Project")

(gc "FSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "FSharp\Reports\dotCover.xml" -Encoding UTF8
(gc "FSharp\Reports\dotCover.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "FSharp\Reports\dotCover.xml" -Encoding UTF8