# Sample F# microservices

This is a sample F# service split across two containers one Web API and one worker.

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