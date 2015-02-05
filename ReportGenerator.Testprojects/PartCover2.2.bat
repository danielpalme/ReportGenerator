@echo off

regsvr32.exe /s Tools\PartCover2.2\PartCover.CorDriver.dll

"Tools\PartCover2.2\PartCover.exe" --target "CSharp\Test\bin\Debug\Test.exe" --include "[Test]*" --output Partcover2.2_CSharp.xml

"Tools\PartCover2.2\PartCover.exe" --target "FSharp\FSharpTest\bin\Debug\FSharpTest.exe" --output Partcover2.2_FSharp.xml

regsvr32.exe /u /s Tools\PartCover2.2\PartCover.CorDriver.dll