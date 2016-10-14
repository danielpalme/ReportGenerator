New-Item -ItemType Directory -Force Java\bin
New-Item -ItemType Directory -Force Java\bin\classes
javac -d Java\bin\classes Java\Project\*.java
 
".\..\packages\CoverageTools\apache-ant-1.9.7\bin\ant" -f ".\JaCoCo_Ant.xml"

Remove-Item Java\bin -Force -Recurse
Remove-Item jacoco.exec