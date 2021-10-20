#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["dependencies.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.Api/NBB.Contracts.Api.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.Api/"]
COPY ["src/Messaging/NBB.Messaging.Rusi/NBB.Messaging.Rusi.csproj", "src/Messaging/NBB.Messaging.Rusi/"]
COPY ["src/Messaging/NBB.Messaging.Abstractions/NBB.Messaging.Abstractions.csproj", "src/Messaging/NBB.Messaging.Abstractions/"]
COPY ["src/Correlation/NBB.Correlation/NBB.Correlation.csproj", "src/Correlation/NBB.Correlation/"]
COPY ["src/Messaging/NBB.Messaging.DataContracts/NBB.Messaging.DataContracts.csproj", "src/Messaging/NBB.Messaging.DataContracts/"]
COPY ["src/Core/NBB.Core.Pipeline/NBB.Core.Pipeline.csproj", "src/Core/NBB.Core.Pipeline/"]
COPY ["src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj", "src/Core/NBB.Core.Abstractions/"]
COPY ["src/Messaging/NBB.Messaging.Nats/NBB.Messaging.Nats.csproj", "src/Messaging/NBB.Messaging.Nats/"]
COPY ["src/Correlation/NBB.Correlation.AspNet/NBB.Correlation.AspNet.csproj", "src/Correlation/NBB.Correlation.AspNet/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/NBB.Contracts.PublishedLanguage.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/"]
COPY ["src/Messaging/NBB.Messaging.BackwardCompatibility/NBB.Messaging.BackwardCompatibility.csproj", "src/Messaging/NBB.Messaging.BackwardCompatibility/"]
COPY ["src/Messaging/NBB.Messaging.OpenTracing/NBB.Messaging.OpenTracing.csproj", "src/Messaging/NBB.Messaging.OpenTracing/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel.Data/NBB.Contracts.ReadModel.Data.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel.Data/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/NBB.Contracts.ReadModel.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/"]
COPY ["src/Data/NBB.Data.EntityFramework/NBB.Data.EntityFramework.csproj", "src/Data/NBB.Data.EntityFramework/"]
COPY ["src/Data/NBB.Data.Abstractions/NBB.Data.Abstractions.csproj", "src/Data/NBB.Data.Abstractions/"]
RUN dotnet restore "samples/MicroServices/NBB.Contracts/NBB.Contracts.Api/NBB.Contracts.Api.csproj"
COPY . .
WORKDIR "/src/samples/MicroServices/NBB.Contracts/NBB.Contracts.Api"
RUN dotnet build "NBB.Contracts.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NBB.Contracts.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NBB.Contracts.Api.dll"]
