FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
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