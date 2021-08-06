# NBB.MultiTenancy.Abstractions

This package provides some abstractions for multi-tenant projects like
* `Tenant` - data structure that stores tenant information (eg. id, name, code)
* `Tenant context` - holds and provides access to the current tenant
* `Tenant repository` - provides functionality to retrieve tenant information from a store
* `Tenancy options` - configuration options for multi-tenant applications

## NuGet install
```
dotnet add package NBB.MultiTenancy.Abstractions
```

