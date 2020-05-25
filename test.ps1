# dotnet test --results-directory:TestResults --collect:"Code Coverage"
dotnet test /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=opencover