@echo off

regsvr32.exe /s Tools\PartCover2.3.35109\PartCover.CorDriver.dll

"Tools\PartCover2.3.35109\PartCover.exe" --target "CSharp\Test\bin\Debug\Test.exe" --include "[Test]*" --output Partcover2.3.35109_CSharp.xml

"Tools\PartCover2.3.35109\PartCover.exe" --target "FSharp\FSharpTest\bin\Debug\FSharpTest.exe" --output Partcover2.3.35109_FSharp.xml

regsvr32.exe /u /s Tools\PartCover2.3.35109\PartCover.CorDriver.dll