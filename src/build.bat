msbuild .\Testprojects\CSharp\Project\Test_CSharp.csproj /t:Restore /v:m
msbuild .\build.proj /t:Restore /v:m
msbuild .\build.proj /v:m