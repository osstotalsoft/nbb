# NBB.MultiTenancy.Identification.Messaging

This package provides tenant identification from messaging envelopes:
* `TenantIdHeaderMessagingTokenResolver` - obtains the tenant token from the given message header

For details about resolvers see * `Identification service` - obtains the tenant ID from the context (includes abstraction and default implementation)
* `Identification strategy` - the default tenant identification service tries to apply a list of identification strategies to obtain the tenant ID. A strategy uses tenant resolvers and identifiers.
* `Tenant resolver` - obtains a tenant token from the context. The tenant token is a string that can be used to identify a tenant.
* `Tenant identifier`- obtains the tenant ID from the tenant token.* `Identification service` - obtains the tenant ID from the context (includes abstraction and default implementation)
* `Identification strategy` - the default tenant identification service tries to apply a list of identification strategies to obtain the tenant ID. A strategy uses tenant resolvers and identifiers.
* `Tenant resolver` - obtains a tenant token from the context. The tenant token is a string that can be used to identify a tenant.
* `Tenant identifier`- obtains the tenant ID from the tenant token.

## NuGet install
```
dotnet add package NBB.MultiTenancy.Identification.Messaging
```

## Default strategy registration
The default HTTP identification strategy tries to obtain the tenant id from "nbb-tenantID" messaging header

Registration:
```csharp
services.AddDefaultMessagingTenantIdentification()
```
