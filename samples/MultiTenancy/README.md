# Multi-tenant todo list service

This is a sample multi-tenant service containing split across two containers one Web API and one worker.

## Configuration

Depending on the configuration this service can be deployed in one of the three possible multi-tenancy options:
* `None` - deployed in a no tenancy environment
* `MultiTenant` - shared deployment in a multi-tenant environment
* `MonoTenant` - dedicated deployment bound to a single tenant

You can configure the tenancy hosting options in *appsettings.json*:
```json
"MultiTenancy": {
    "TenancyType": "MultiTenant", // "None" "MultiTenant" "MonoTenant"
    "TenantId": "675a4df2-cd58-4320-bb78-9c7898a03ab7"
}
```

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
## Database migrations
To migrate the database you need to execute the migrations project
```
dotnet run --project .\samples\MultiTenancy\NBB.Todo.Migrations\NBB.Todo.Migrations.csproj
```

The migration project is configured to create a multi-tenant shared database.

## Running the sample
You need to start both the API and Worker projects. 

Currently there are two use cases:
* query the todo list
* create a todo task

you can use the following Postman collection to execute the requests: [`TODO sample`](MultiTenantTodoList.postman_collection.json)