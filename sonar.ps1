dotnet C:\Users\dsimionescu\Downloads\sonar-scanner-msbuild-4.9.0.17385-netcoreapp2.0\SonarScanner.MSBuild.dll begin /k:"nbb" /d:sonar.host.url="https://sonarqube.appservice.online" /d:sonar.login="d357768ec27ec5b9435a023f6e6df11be4aa0ac0" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.coverage.exclusions="samples/**/*" /d:sonar.exclusions="template/**/*"

#/d:sonar.cs.vscoveragexml.reportsPaths="**\*.coveragexml"

dotnet clean .\NBB.sln
dotnet clean .\NBB.ProcessManager.sln

dotnet restore .\NBB.sln
dotnet restore .\NBB.ProcessManager.sln

dotnet build .\NBB.sln
dotnet build .\NBB.ProcessManager.sln

#dotnet test --collect:"XPlat Code Coverage" --settings testrunsettings.xml
dotnet test NBB.sln /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=opencover
dotnet test NBB.ProcessManager.sln /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=opencover

# dotnet test --collect "code coverage"
dotnet C:\Users\dsimionescu\Downloads\sonar-scanner-msbuild-4.9.0.17385-netcoreapp2.0\SonarScanner.MSBuild.dll end /d:sonar.login="d357768ec27ec5b9435a023f6e6df11be4aa0ac0"


# dotnet test .\NBB.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutputDirectory=D:\Git\nbb\coverage