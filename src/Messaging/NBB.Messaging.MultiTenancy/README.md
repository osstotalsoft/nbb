# NBB.Messaging.MultiTenancy

This package provides multi-tenancy functionality for the messaging infrastructure.
## NuGet install
```
dotnet add package NBB.MultiTenancy.AspNet
```
## Services

To register the multi-tenant messaging services use the following extension method:

```csharp
services.AddMultiTenantMessaging();
```

## Messaging host middleware

The package provides a message host middleware that is responsible with: 
* identifying the tenant identifier from the incoming request
* loading tenant information from the repository
* building the tenancy context that holds the current tenant

### Registration
The tenancy middleware can be registered in `Startup.cs` using the following application builder extension:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseTenantMiddleware();
    ...
}
```

### Contract classification
Events (e.g. notifications) whose tenant cannot be identified, or whose tenant is not found or disabled, are skipped; any other message (commands, queries) requires a valid tenant.
The middleware decides what an event is using the application's `IContractKindClassifier` (`NBB.Core.Abstractions`), registered by the mediator application adapter:

```csharp
services.AddMediatorIntegration(); // NBB.Application.Mediator
// or
services.AddMediatRIntegration();  // NBB.Application.MediatR
```

Without a classifier the middleware cannot be activated (the first message fails with a dependency injection error).
