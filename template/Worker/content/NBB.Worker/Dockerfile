FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY NuGet.config .
COPY dependencies.props .
COPY Directory.Build.props .
COPY ["src/NBB.Worker/NBB.Worker.csproj", "src/NBB.Worker/"]
RUN dotnet restore "src/NBB.Worker/NBB.Worker.csproj"
COPY ["src/NBB.Worker/", "src/NBB.Worker/"]
WORKDIR "/src/src/NBB.Worker"
RUN dotnet build "NBB.Worker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "NBB.Worker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NBB.Worker.dll"]