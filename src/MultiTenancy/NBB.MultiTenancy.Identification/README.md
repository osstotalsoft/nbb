# NBB.MultiTenancy.Identification

This package provides abstractions for tenant identification like:
* `Identification service` - obtains the tenant ID from the context (includes abstraction and default implementation)
* `Identification strategy` - the default tenant identification service tries to apply a list of identification strategies to obtain the tenant ID. A strategy uses tenant resolvers and identifiers.
* `Tenant resolver` - obtains a tenant token from the context. The tenant token is a string that can be used to identify a tenant.
* `Tenant identifier`- obtains the tenant ID from the tenant token.

## NuGet install
```
dotnet add package NBB.MultiTenancy.Identification
```

## Registration
The following example registers a tenant identification service that uses the following strategy:
* try to get a tenant token using the resolver JwtBearerTokenResolver (from Jwt token), from the claim "tid"
* try to get a tenant token using the resolver `HeaderHttpTokenResolver` from the header key "TenantId"
* if a tenant token could not be resolved in the previous step, try to get one using the resolver `QueryStringHttpTokenResolver`, from the key "TenantId"
* if a token was resolved, obtain the tenant ID using the identifier `IdTenantIdentifier`

```csharp
    services.AddTenantIdentificationService()
        .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
            .AddTenantTokenResolver<JwtBearerTokenResolver>(DefaultJwtClaimStringParamName)
            .AddTenantTokenResolver<HeaderHttpTokenResolver>(DefaultTenantHttpHeaderName)
            .AddTenantTokenResolver<QueryStringHttpTokenResolver>(DefaultTenantQueryStringParamName)
        );
```
