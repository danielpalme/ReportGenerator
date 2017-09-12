@echo off

"CoverageTools\NCover\NCover.Console.exe" //reg //a Test "CSharp\Test\bin\Debug\Test.exe" //x "Ncover1.5.8_CSharp.xml"

del Coverage.Log