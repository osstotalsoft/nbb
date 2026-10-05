# Multi-tenant todo list service

This is a sample multi-tenant service split across two processes, a Web API and a messaging worker, plus a migrations project.

## Configuration

Depending on the configuration this service can be deployed in one of the two possible multi-tenancy options:
* `MultiTenant` - shared deployment in a multi-tenant environment
* `MonoTenant` - dedicated deployment without tenant-specific functionality

You can configure the tenancy hosting options in *appsettings.json*:
```json
"MultiTenancy": {
    "TenancyType": "MultiTenant", // "MultiTenant" "MonoTenant"
    "Defaults": {
      "ConnectionStrings": {
          "DefaultConnection" : "Server=YOUR_SERVER;Database=NBB_Todo;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true"
      }
    },
    "Tenants": {
      "tenant1": {
        "TenantId": "f7bfa571-4067-4167-a4c5-dafb71ccdcf7"
      },
      "tenant2": {
        "TenantId": "a7bfa571-4067-4167-a4c5-dafb71ccdcf7"
      }
    }
}
```

The tenants inherit the `Defaults` connection string, so in this sample they share the same database.

The tenant is identified from the `TenantId` HTTP header (or the `tenantId` query string parameter) on `/api` requests, and from the message headers in the worker.

## Running the sample with Aspire

[`NBB.Todo.AppHost`](./NBB.Todo.AppHost) is an [Aspire](https://aspire.dev) AppHost that runs the migrations, the API and the worker together, with the Aspire dashboard for logs, traces and metrics.

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Aspire CLI](https://aspire.dev/get-started/install-cli/)
* a SQL Server instance; the login must be able to create databases
* a NATS server with JetStream enabled, with a stream whose subjects cover the sample topics (prefixed with `Messaging:Env`, which is `DEV` by default):
  ```
  nats stream add NBB_DEV --subjects "DEV.>" --defaults
  ```

### Configuration
* `ConnectionStrings:sql` - a server-level SQL Server connection string, **without** `Database=`. The AppHost derives the `NBB_Todo` connection string from it. Keep it in the AppHost user secrets:
  ```
  dotnet user-secrets set ConnectionStrings:sql "Server=YOUR_SERVER;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True" --project samples/MultiTenancy/NBB.Todo.AppHost
  ```
* `ConnectionStrings:jetstream` - the NATS server URL, configured in [`appsettings.json`](./NBB.Todo.AppHost/appsettings.json) of the AppHost.

### Running
From the `samples/MultiTenancy` folder:
```
aspire start
```
The migrations run first (mono-tenant) and create or update the database; the API and worker start after they complete. Open the dashboard link printed by `aspire start`, and use `aspire stop` to stop everything.

## Running the projects on their own
Each project can also be started individually. It then reads its configuration from its own `appsettings.json` and user secrets: `MultiTenancy:Defaults:ConnectionStrings:DefaultConnection` (`ConnectionStrings:DefaultConnection` for the mono-tenant migrations) and the NATS server URL:
```json
"Messaging": {
    "JetStream": {
        "natsUrl": "<your NATS URL>"
    }
}
```

Run the migrations first:
```
dotnet run --project .\samples\MultiTenancy\NBB.Todo.Migrations\NBB.Todo.Migrations.csproj
```
then start both the API and Worker projects.

## Trying it out
Currently there are two use cases:
* query the todo list
* create a todo task

The API listens on http://localhost:58733. You can use the following Postman collection to execute the requests: [`TODO sample`](MultiTenantTodoList.postman_collection.json)
