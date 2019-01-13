New-Item -ItemType Directory -Force Java\bin
New-Item -ItemType Directory -Force Java\bin\classes
New-Item -ItemType Directory -Force Java\bin\instrumented

java -cp CoverageTools\OpenClover\clover.jar com.atlassian.clover.CloverInstr -i clover.db -s Java\Project -d Java\bin\instrumented

javac -cp CoverageTools\OpenClover\clover.jar -d Java\bin\classes Java\bin\instrumented\test\*.java
javac -cp CoverageTools\OpenClover\clover.jar -d Java\bin\classes Java\bin\instrumented\test\sub\*.java
  
java -cp "CoverageTools\OpenClover\clover.jar;Java\bin\classes" test.Program
 
java -cp CoverageTools\OpenClover\clover.jar com.atlassian.clover.reporters.xml.XMLReporter -l -i clover.db -o Java\Reports\Clover_OpenClover4.3.1.xml

Remove-Item clover.db*

$pathToReplace = [regex]::Escape((Get-Location).Path + "\Java\Project")
$replacement = "C:\temp"

(gc "Java\Reports\Clover_OpenClover4.3.1.xml") | % { $_ -replace $pathToReplace, $replacement } | Out-File "Java\Reports\Clover_OpenClover4.3.1.xml" -Encoding UTF8

Remove-Item Java\bin -Force -Recurse