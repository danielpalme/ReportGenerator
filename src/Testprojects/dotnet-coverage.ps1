dotnet tool install --global dotnet-coverage
dotnet-coverage collect -f cobertura "dotnet test CSharp\Project_DotNetCore\UnitTests\UnitTests.csproj"