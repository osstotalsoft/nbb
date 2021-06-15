# Multi-tenant todo list service

This is a sample multi-tenant service containing split across two containers one Web API and one worker.

## Configuration

Depending on the configuration this service can be deployed in one of the three possible multi-tenancy options:
* `MultiTenant` - shared deployment in a multi-tenant environment
* `MonoTenant` - dedicated deployment without tenant-speciffic functionality

You can configure the tenancy hosting options in *appsettings.json*:
```json
"MultiTenancy": {
    "TenancyType": "MultiTenant", // "MultiTenant" "MonoTenant"
    "Defaults": {
      "ConnectionString": "Server=YOUR_SERVER;Database=NBB_Invoices;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true"
    },
    "Tenants": [
      {
        "TenantId": "f7bfa571-4067-4167-a4c5-dafb71ccdcf7",
        "Code": "tenant1"
      },
      {
        "TenantId": "a7bfa571-4067-4167-a4c5-dafb71ccdcf7",
        "Code": "tenant2"
      }
    ]
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

## Database migrations
To migrate the database you need to execute the migrations project
```
dotnet run --project .\samples\MultiTenancy\NBB.Todo.Migrations\NBB.Todo.Migrations.csproj
```


## Running the sample
You need to start both the API and Worker projects. 

Currently there are two use cases:
* query the todo list
* create a todo task

you can use the following Postman collection to execute the requests: [`TODO sample`](MultiTenantTodoList.postman_collection.json)