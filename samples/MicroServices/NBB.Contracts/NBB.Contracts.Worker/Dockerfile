#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["dependencies.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.Worker/NBB.Contracts.Worker.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.Worker/"]
COPY ["src/Messaging/NBB.Messaging.Host/NBB.Messaging.Host.csproj", "src/Messaging/NBB.Messaging.Host/"]
COPY ["src/Messaging/NBB.Messaging.Abstractions/NBB.Messaging.Abstractions.csproj", "src/Messaging/NBB.Messaging.Abstractions/"]
COPY ["src/Correlation/NBB.Correlation/NBB.Correlation.csproj", "src/Correlation/NBB.Correlation/"]
COPY ["src/Messaging/NBB.Messaging.DataContracts/NBB.Messaging.DataContracts.csproj", "src/Messaging/NBB.Messaging.DataContracts/"]
COPY ["src/Core/NBB.Core.Pipeline/NBB.Core.Pipeline.csproj", "src/Core/NBB.Core.Pipeline/"]
COPY ["src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj", "src/Core/NBB.Core.Abstractions/"]
COPY ["src/Core/NBB.Core.Effects/NBB.Core.Effects.csproj", "src/Core/NBB.Core.Effects/"]
COPY ["src/Messaging/NBB.Messaging.Rusi/NBB.Messaging.Rusi.csproj", "src/Messaging/NBB.Messaging.Rusi/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.Application/NBB.Contracts.Application.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.Application/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/NBB.Contracts.ReadModel.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/"]
COPY ["src/EventStore/NBB.EventStore.Abstractions/NBB.EventStore.Abstractions.csproj", "src/EventStore/NBB.EventStore.Abstractions/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.Domain/NBB.Contracts.Domain.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.Domain/"]
COPY ["src/Domain/NBB.Domain/NBB.Domain.csproj", "src/Domain/NBB.Domain/"]
COPY ["src/Domain/NBB.Domain.Abstractions/NBB.Domain.Abstractions.csproj", "src/Domain/NBB.Domain.Abstractions/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/NBB.Contracts.PublishedLanguage.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/"]
COPY ["src/Application/NBB.Application.MediatR/NBB.Application.MediatR.csproj", "src/Application/NBB.Application.MediatR/"]
COPY ["src/Data/NBB.Data.Abstractions/NBB.Data.Abstractions.csproj", "src/Data/NBB.Data.Abstractions/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.WriteModel.Data/NBB.Contracts.WriteModel.Data.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.WriteModel.Data/"]
COPY ["src/Data/NBB.Data.EventSourcing/NBB.Data.EventSourcing.csproj", "src/Data/NBB.Data.EventSourcing/"]
COPY ["src/EventStore/NBB.EventStore/NBB.EventStore.csproj", "src/EventStore/NBB.EventStore/"]
COPY ["src/Messaging/NBB.Messaging.Nats/NBB.Messaging.Nats.csproj", "src/Messaging/NBB.Messaging.Nats/"]
COPY ["src/EventStore/NBB.EventStore.AdoNet/NBB.EventStore.AdoNet.csproj", "src/EventStore/NBB.EventStore.AdoNet/"]
COPY ["src/Correlation/NBB.Correlation.Serilog/NBB.Correlation.Serilog.csproj", "src/Correlation/NBB.Correlation.Serilog/"]
COPY ["src/Messaging/NBB.Messaging.BackwardCompatibility/NBB.Messaging.BackwardCompatibility.csproj", "src/Messaging/NBB.Messaging.BackwardCompatibility/"]
COPY ["src/Messaging/NBB.Messaging.OpenTracing/NBB.Messaging.OpenTracing.csproj", "src/Messaging/NBB.Messaging.OpenTracing/"]
COPY ["samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel.Data/NBB.Contracts.ReadModel.Data.csproj", "samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel.Data/"]
COPY ["src/Data/NBB.Data.EntityFramework/NBB.Data.EntityFramework.csproj", "src/Data/NBB.Data.EntityFramework/"]
RUN dotnet restore "samples/MicroServices/NBB.Contracts/NBB.Contracts.Worker/NBB.Contracts.Worker.csproj"
COPY . .
WORKDIR "/src/samples/MicroServices/NBB.Contracts/NBB.Contracts.Worker"
RUN dotnet build "NBB.Contracts.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NBB.Contracts.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NBB.Contracts.Worker.dll"]
