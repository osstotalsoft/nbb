# Sample F# microservices

This is a sample F# service split across two containers one Web API and one worker.

This sample shows the usage of:
- Pure functional domain modelling using DDD concepts
- Evented domain entities powered by the [`Evented computation expression`](../../../src/Core/NBB.Core.Evented.FSharp#README.md)
- Clean architecture with DI powered by the [`Effect computation expression`](../../../src/Core/NBB.Core.Effects.FSharp#README.md)
- Pure domain and application layer powered by the [`Effect computation expression`](../../../src/Core/NBB.Core.Effects.FSharp#README.md)
- Application use-cases and pipelines powered by [`NBB.Application.Mediator.FSharp`](../../../src/Application/NBB.Application.Mediator.FSharp#README.md)
- Web api powered by [`Giraffe`](https://github.com/giraffe-fsharp/Giraffe)
- Messaging worker powered by [`NBB.Messaging.Host`](../../../src/Messaging/NBB.Messaging.Host#README.md) and [`NBB.Messaging.Effects`](../../../src/Messaging/NBB.Messaging.Effects#README.md)


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