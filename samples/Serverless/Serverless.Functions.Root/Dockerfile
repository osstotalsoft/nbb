FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY samples/Serverless/Serverless.Functions.Root/Serverless.Functions.Root.csproj samples/Serverless/Serverless.Functions.Root/
COPY samples/Serverless/NBB.Contracts.Functions.CreateContract/NBB.Contracts.Functions.CreateContract.csproj samples/Serverless/NBB.Contracts.Functions.CreateContract/
COPY src/EventStore/NBB.EventStore/NBB.EventStore.csproj src/EventStore/NBB.EventStore/
COPY src/EventStore/NBB.EventStore.Abstractions/NBB.EventStore.Abstractions.csproj src/EventStore/NBB.EventStore.Abstractions/
COPY src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj src/Core/NBB.Core.Abstractions/
COPY samples/MicroServices/NBB.Contracts/NBB.Contracts.Data/NBB.Contracts.Data.csproj samples/MicroServices/NBB.Contracts/NBB.Contracts.Data/
COPY src/Data/NBB.Data.EntityFramework/NBB.Data.EntityFramework.csproj src/Data/NBB.Data.EntityFramework/
COPY src/Domain/NBB.Domain.Abstractions/NBB.Domain.Abstractions.csproj src/Domain/NBB.Domain.Abstractions/
COPY samples/MicroServices/NBB.Contracts/NBB.Contracts.Domain/NBB.Contracts.Domain.csproj samples/MicroServices/NBB.Contracts/NBB.Contracts.Domain/
COPY src/Domain/NBB.Domain/NBB.Domain.csproj src/Domain/NBB.Domain/
COPY samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/NBB.Contracts.ReadModel.csproj samples/MicroServices/NBB.Contracts/NBB.Contracts.ReadModel/
COPY src/Data/NBB.Data.EventSourcing/NBB.Data.EventSourcing.csproj src/Data/NBB.Data.EventSourcing/
COPY src/Messaging/NBB.Messaging.Nats/NBB.Messaging.Nats.csproj src/Messaging/NBB.Messaging.Nats/
COPY src/Messaging/NBB.Messaging.Kafka/NBB.Messaging.Kafka.csproj src/Messaging/NBB.Messaging.Kafka/
COPY src/Messaging/NBB.Messaging.Abstractions/NBB.Messaging.Abstractions.csproj src/Messaging/NBB.Messaging.Abstractions/
COPY src/Messaging/NBB.Messaging.DataContracts/NBB.Messaging.DataContracts.csproj src/Messaging/NBB.Messaging.DataContracts/
COPY src/EventStore/NBB.EventStore.AdoNet/NBB.EventStore.AdoNet.csproj src/EventStore/NBB.EventStore.AdoNet/
COPY src/EventStore/NBB.EventStore.MessagingExtensions/NBB.EventStore.MessagingExtensions.csproj src/EventStore/NBB.EventStore.MessagingExtensions/
COPY src/Monitoring/NBB.Monitoring.Correlation/NBB.Monitoring.Correlation.csproj src/Monitoring/NBB.Monitoring.Correlation/
COPY samples/MicroServices/NBB.Contracts/NBB.Contracts.Application/NBB.Contracts.Application.csproj samples/MicroServices/NBB.Contracts/NBB.Contracts.Application/
COPY samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/NBB.Contracts.PublishedLanguage.csproj samples/MicroServices/NBB.Contracts/NBB.Contracts.PublishedLanguage/
COPY src/Application/NBB.Application/NBB.Application.csproj src/Application/NBB.Application/
COPY src/Messaging/NBB.Messaging.Kafka/NBB.Messaging.Kafka.csproj src/Messaging/NBB.Messaging.Kafka/
COPY src/Mediator/NBB.Mediator.OpenFaaS/NBB.Mediator.OpenFaaS.csproj src/Mediator/NBB.Mediator.OpenFaaS/
COPY src/Monitoring/NBB.Monitoring.Correlation.Serilog/NBB.Monitoring.Correlation.Serilog.csproj src/Monitoring/NBB.Monitoring.Correlation.Serilog/
RUN dotnet restore
COPY . .
WORKDIR /src/samples/Serverless/Serverless.Functions.Root
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
RUN apt-get update -qy \
    && apt-get install -qy curl ca-certificates --no-install-recommends \ 
    && echo "Pulling watchdog binary from Github." \
    && curl -sSL https://github.com/openfaas-incubator/of-watchdog/releases/download/0.2.1/of-watchdog > /usr/bin/fwatchdog \
    && chmod +x /usr/bin/fwatchdog \
    && apt-get -qy remove curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app .

ENV write_debug=true
ENV read_debug=true
ENV afterburn=true
ENV mode=afterburn

ENV fprocess="dotnet ./Serverless.Functions.Root.dll"
EXPOSE 8080
CMD ["fwatchdog"]
