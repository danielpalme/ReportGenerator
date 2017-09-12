@echo off

regsvr32.exe /s CoverageTools\PartCover2.3\PartCover.CorDriver.dll

"CoverageTools\PartCover2.3\PartCover.exe" --target "CSharp\Test\bin\Debug\Test.exe" --include "[Test]*" --output Partcover2.3_CSharp.xml

regsvr32.exe /u /s CoverageTools\PartCover2.3\PartCover.CorDriver.dll