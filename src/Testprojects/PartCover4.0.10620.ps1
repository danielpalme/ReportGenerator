regsvr32.exe /s CoverageTools\PartCover4.0.10620\PartCover.dll

CoverageTools\PartCover4.0.10620\PartCover.exe --target CSharp\Project\bin\Debug\Test.exe --include "[Test]*" --output CSharp\Reports\Partcover4.0.10620.xml
CoverageTools\PartCover4.0.10620\PartCover.exe --target FSharp\Project\bin\Debug\Test.exe --include "[Test]*" --output FSharp\Reports\Partcover4.0.10620.xml 

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\CSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\CSharp\Project")
$replacement = "C:\temp"

(gc "CSharp\Reports\Partcover4.0.10620.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "CSharp\Reports\Partcover4.0.10620.xml" -Encoding UTF8
(gc "CSharp\Reports\Partcover4.0.10620.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "CSharp\Reports\Partcover4.0.10620.xml" -Encoding UTF8

$pathToReplace1 = [regex]::Escape((Get-Location).Path + "\FSharp\Project\bin\Debug")
$pathToReplace2 = [regex]::Escape((Get-Location).Path + "\FSharp\Project")

(gc "FSharp\Reports\Partcover4.0.10620.xml") | % { $_ -replace $pathToReplace1, $replacement } | Out-File "FSharp\Reports\Partcover4.0.10620.xml" -Encoding UTF8
(gc "FSharp\Reports\Partcover4.0.10620.xml") | % { $_ -replace $pathToReplace2, $replacement } | Out-File "FSharp\Reports\Partcover4.0.10620.xml" -Encoding UTF8

regsvr32.exe /u /s CoverageTools\PartCover4.0.10620\PartCover.dll