msbuild .\Testprojects\CSharp\Project\Test_CSharp.csproj /t:Restore /v:q
msbuild .\build.proj /t:Restore /v:q
msbuild .\build.proj /v:m