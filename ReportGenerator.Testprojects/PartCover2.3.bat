@echo off

regsvr32.exe /s Tools\PartCover2.3\PartCover.CorDriver.dll

"Tools\PartCover2.3\PartCover.exe" --target "CSharp\Test\bin\Debug\Test.exe" --include "[Test]*" --output Partcover2.3_CSharp.xml

regsvr32.exe /u /s Tools\PartCover2.3\PartCover.CorDriver.dll