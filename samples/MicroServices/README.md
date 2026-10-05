Microservices samples
===============

The microservices samples show different approaches to building microservices using NBB:
* [`NBB.Contracts`](./NBB.Contracts) - event sourced contracts service with a separate read model
* [`NBB.Invoices`](./NBB.Invoices) - invoices service with EF Core persistence and domain events
* [`NBB.Payments`](./NBB.Payments) - payables and payments service
* [`NBB.MicroServicesOrchestration`](./NBB.MicroServicesOrchestration) - process manager that orchestrates the invoicing flow across the services above
* [`NBB.Invoices.FSharp`](./NBB.Invoices.FSharp#readme) - the invoices service written in F#

Each service is split into a Web API, a messaging worker and a migrations project. The services communicate through the NBB message bus, using the NATS JetStream transport.

## Running the samples with Aspire

[`NBB.MicroServices.AppHost`](./NBB.MicroServices.AppHost) is an [Aspire](https://aspire.dev) AppHost that runs the Contracts, Invoices and Payments services and the orchestration process manager together, with the Aspire dashboard for logs, traces and metrics. `NBB.Invoices.FSharp` is not part of the AppHost.

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Aspire CLI](https://aspire.dev/get-started/install-cli/)
* a SQL Server instance; the login must be able to create databases
* a NATS server with JetStream enabled, with a stream whose subjects cover the sample topics (the topics are prefixed with `Messaging:Env`, which is `DEV` by default):
  ```
  nats stream add NBB_DEV --subjects "DEV.>" --defaults
  ```

### Configuration
The AppHost takes two connection strings and passes them to the services:

* `ConnectionStrings:sql` - a server-level SQL Server connection string, **without** `Database=`. The AppHost derives the `NBB_Contracts`, `NBB_Invoices` and `NBB_Payments` connection strings from it. Keep it in the AppHost user secrets:
  ```
  dotnet user-secrets set ConnectionStrings:sql "Server=YOUR_SERVER;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True" --project samples/MicroServices/NBB.MicroServices.AppHost
  ```
* `ConnectionStrings:jetstream` - the NATS server URL, configured in [`appsettings.json`](./NBB.MicroServices.AppHost/appsettings.json) of the AppHost (override it in the AppHost user secrets if needed).

### Running
From the `samples/MicroServices` folder:
```
aspire start
```
The migrations projects run first and create or update the databases; the APIs and workers start after their migrations complete. The migrations are non-destructive, so existing data is kept between runs.

Open the dashboard link printed by `aspire start` to see the resources, logs and traces. Use `aspire stop` to stop everything.

### Trying it out
Use the [`NBB Samples`](./NBB%20Samples.postman_collection.json) Postman collection. The APIs listen on:

| Service | URL |
|---|---|
| Contracts API | http://localhost:2047/api/contracts |
| Invoices API | http://localhost:2048/api/invoices |
| Payments API | http://localhost:2046/api/payables |

A typical flow: create a contract, add a contract line and validate the contract. The orchestration process manager then creates an invoice and a payable; paying the payable marks the invoice as paid. The whole flow shows up as a single distributed trace in the dashboard.

> **Note:** when several developers use the same NATS server, the services share durable consumers (named after `Messaging:JetStream:clientId` and the topic) and can receive each other's messages.

## Running a service on its own
Each service can still be started individually (for example with `dotnet run` or from the IDE). It then reads its configuration from its own `appsettings.json` and user secrets instead of the AppHost:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<the service database connection string>"
  },
  "EventStore": {
    "NBB": {
      "ConnectionString": "<the service database connection string>"
    }
  },
  "Messaging": {
    "JetStream": {
      "natsUrl": "<your NATS URL>"
    }
  }
}
```

`EventStore:NBB:ConnectionString` is needed by the workers, the migrations and the orchestration process manager, which keeps its event store in the `NBB_Invoices` database.
