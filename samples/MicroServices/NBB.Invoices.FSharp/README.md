# Sample F# microservices

This is a sample F# service split across two processes, a Web API and a messaging worker. It is a drop-in replacement for the C# [`NBB.Invoices`](../NBB.Invoices) service:
- it serves the same API (`GET /api/invoices`, `GET /api/invoices/{id}`, `POST /api/invoices`, `POST /api/invoices/{id}/process`) on the same port
- it handles the same [`NBB.Invoices.PublishedLanguage`](../NBB.Invoices/NBB.Invoices.PublishedLanguage) commands (`CreateInvoice`, `MarkInvoiceAsPayed`, `ProcessInvoice`) and publishes the same integration events (`InvoiceCreated`, `InvoiceMarkedAsPayed`), so the orchestration process manager works with either implementation
- it stores the invoices in the same `Invoices` table of the `NBB_Invoices` database, created by [`NBB.Invoices.Migrations`](../NBB.Invoices/NBB.Invoices.Migrations)

This sample shows the usage of:
- Pure functional domain modelling using DDD concepts
- Evented domain entities powered by the [`Evented computation expression`](../../../src/Core/NBB.Core.Evented.FSharp#README.md)
- Clean architecture with DI powered by the [`Effect computation expression`](../../../src/Core/NBB.Core.Effects.FSharp#README.md)
- Pure domain and application layer powered by the [`Effect computation expression`](../../../src/Core/NBB.Core.Effects.FSharp#README.md)
- Application use-cases and pipelines powered by [`NBB.Application.Mediator.FSharp`](../../../src/Application/NBB.Application.Mediator.FSharp#README.md)
- Web api powered by [`Giraffe`](https://github.com/giraffe-fsharp/Giraffe)
- Messaging worker powered by [`NBB.Messaging.Host`](../../../src/Messaging/NBB.Messaging.Host#README.md) and [`NBB.Messaging.Effects`](../../../src/Messaging/NBB.Messaging.Effects#README.md)

## Running with Aspire
Set `Invoices:UseFSharp` to `true` in the [`NBB.MicroServices.AppHost`](../NBB.MicroServices.AppHost/appsettings.json) configuration (or start it with the `Invoices__UseFSharp=true` environment variable). The AppHost then runs these projects as `invoices-api` and `invoices-worker` instead of the C# ones; see the [microservices samples](../README.md) for the rest of the setup.

## Running on its own
You need to configure your NATS server URL like this:
```json
"Messaging": {
    "JetStream": {
        "natsUrl": "<your NATS URL>"
    }
}
```
and also need to provide the database connection string:
```json
"ConnectionStrings": {
    "DefaultConnection": "<your connection string>"
}
```

Run the [`NBB.Invoices.Migrations`](../NBB.Invoices/NBB.Invoices.Migrations) project to create the database, then start both the API and Worker projects. You can use the following Postman collection to execute the requests: [`NBB.Invoices.FSharp.postman_collection`](NBB.Invoices.FSharp.postman_collection.json)
