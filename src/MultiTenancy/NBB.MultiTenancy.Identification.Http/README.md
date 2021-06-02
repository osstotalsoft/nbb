# NBB.MultiTenancy.Identification.Http
This package provides tenant identification from HTTP requests:
* `TenantIdHeaderHttpTokenResolver` - obtains the tenant token from the given http header
* `QueryStringTenantIdTokenResolver` - obtains the tenant token from the given query string parameter
* `HostHttpTenantTokenResolver` - obtains the tenant token from http request host 
* `HostRefererHttpTokenResolver` - obtains the tenant token from the host name in the 'Referer' HTTP header. 

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
