@echo off

regsvr32.exe /s CoverageTools\PartCover2.2\PartCover.CorDriver.dll

"CoverageTools\PartCover2.2\PartCover.exe" --target "CSharp\Test\bin\Debug\Test.exe" --include "[Test]*" --output Partcover2.2_CSharp.xml

"CoverageTools\PartCover2.2\PartCover.exe" --target "FSharp\FSharpTest\bin\Debug\FSharpTest.exe" --output Partcover2.2_FSharp.xml

regsvr32.exe /u /s CoverageTools\PartCover2.2\PartCover.CorDriver.dll