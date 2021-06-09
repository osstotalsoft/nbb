# Sample F# microservices

This is a sample F# service split across two containers one Web API and one worker.

This sample demonstrates the usage of:
- Functional domain modelling using DDD concepts
- Evented domain entities powered by the [`Evented computation expression`](../../../src/Core/NBB.Core.Evented.FSharp#README.md)
- Clean architecture with DI powered by the `Effect computation expression`
- Pure domain and application layer powered by the `Effect computation expression`
- Application use-cases and pipelines powered by `NBB.Application.Mediator.FSharp`
- Web api powered `Giraffe`
- Messaging worker powered by `NBB.Messaging.Host` and `NBB.Messaging.Effects`

## Domain


## Application

## Data access

## Web Api

## Messaging


## Configuration
You need to configure your NATS server URL like this:
```json
"Messaging": {
    "Nats": {
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

## Running the sample
You need to start both the API and Worker projects and you can use the following Postman collection to execute the requests: [`NBB.Invoices.FSharp.postman_collection`](NBB.Invoices.FSharp.postman_collection.json)