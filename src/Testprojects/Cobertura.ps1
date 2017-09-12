New-Item -ItemType Directory -Force Java\bin
New-Item -ItemType Directory -Force Java\bin\classes
New-Item -ItemType Directory -Force Java\bin\instrumented

javac -d Java\bin\classes Java\Project\test\*.java
javac -d Java\bin\classes Java\Project\test\sub\*.java
 
CoverageTools\Cobertura\cobertura-2.1.1\cobertura-instrument.bat Java\bin\classes --destination Java\bin\instrumented
 
java -cp "CoverageTools\Cobertura\slf4j-api-1.7.5\slf4j-api-1.7.5.jar;CoverageTools\Cobertura\logback-1.0.13\logback-core-1.0.13.jar;CoverageTools\Cobertura\logback-1.0.13\logback-classic-1.0.13.jar;CoverageTools\Cobertura\cobertura-2.1.1\cobertura-2.1.1.jar;Java\bin\instrumented;Java\bin\classes" test.Program
 
#CoverageTools\Cobertura\cobertura-2.1.1\cobertura-report.bat --format html --datafile Java\bin\cobertura.ser --destination Java\Reports Java\Project
CoverageTools\Cobertura\cobertura-2.1.1\cobertura-report.bat --format xml --destination Java\Reports "$((Get-Location).Path)\Java\Project"

Remove-Item Java\Reports\Cobertura2.1.1.xml
Rename-Item -Path "Java\Reports\coverage.xml" -NewName "Cobertura2.1.1.xml"

$pathToReplace = [regex]::Escape((Get-Location).Path.Replace("\", "/") + "/Java/Project")
$replacement = "C:/temp"

(gc "Java\Reports\Cobertura2.1.1.xml") | % { $_ -replace $pathToReplace, $replacement } | Out-File "Java\Reports\Cobertura2.1.1.xml" -Encoding UTF8

Remove-Item Java\bin -Force -Recurse
Remove-Item cobertura.ser