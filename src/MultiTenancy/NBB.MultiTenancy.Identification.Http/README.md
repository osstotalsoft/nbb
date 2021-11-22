# NBB.MultiTenancy.Identification.Http
This package provides tenant identification from HTTP requests:
* `FormTokenResolver` - obtains the tenant token from the form values (when Request.HasFormContentType == true)
* `HeaderHttpTokenResolver` - obtains the tenant token from the given http header
* `HeaderRegexHttpTokenResolver` - obtains the tenant token from the header with a regular expression.
* `HostHttpTenantTokenResolver` - obtains the tenant token from http request host 
* `HostRefererHttpTokenResolver` - obtains the tenant token from the host name in the 'Referer' HTTP header.
* `JwtBearerTokenResolver` - obtains the tenant token from the a claim in a Jwt token extracted from Bearer authentication.
* `QueryStringTokenResolver` - obtains the tenant token from the given query string parameter

For details about resolvers see [`NBB.Multitenancy.Identification`](../NBB.MultiTenancy.Identification#readme)

## NuGet install
```
dotnet add package NBB.MultiTenancy.Identification.Http
```

## Default strategy registration
The default HTTP identification strategy: 
* first try to obtain the tenant id from "TenantId" HTTP header
* if not found, try to obtain the tenant id from the "tenantId" query string parameter

Registration:
```csharp
services.AddDefaultHttpTenantIdentification()
```
