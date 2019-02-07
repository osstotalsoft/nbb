FROM microsoft/aspnetcore:2.0-jessie
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "NBB.Invoices.Worker.dll"]
