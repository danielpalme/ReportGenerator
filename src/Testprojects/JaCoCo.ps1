New-Item -ItemType Directory -Force Java\bin
New-Item -ItemType Directory -Force Java\bin\classes
javac -d Java\bin\classes Java\Project\test\*.java
javac -d Java\bin\classes Java\Project\test\sub\*.java
 
CoverageTools\JaCoCo\apache-ant-1.9.7\bin\ant -f .\JaCoCo_Ant.xml

Remove-Item Java\bin -Force -Recurse
Remove-Item jacoco.exec