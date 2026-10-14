# NBB.MultiTenancy.Abstractions

This package provides some abstractions for multi-tenant projects like
* `Tenant` - data structure that stores tenant information (eg. id, name, code)
* `Tenant context` - holds and provides access to the current tenant
* `Tenant repository` - provides functionality to retrieve tenant information from a store
* 'Tenant Configuration' - provides tenant specific configuration
* `Tenancy options` - configuration options for multi-tenant applications

## NuGet install
```
dotnet add package NBB.MultiTenancy.Abstractions
```

## Tenant Configuration

### Abstractions:
The `ITennantConfiguration` derives from the `IConfiguration` so that you can use all the well known `Microsoft.Extensions.Configuration` apis

```csharp
namespace NBB.MultiTenancy.Abstractions.Configuration;
public interface ITenantConfiguration : IConfiguration
{
}
```
### Extensions:
The `GetConnectionString` extension:
```csharp
public static string GetConnectionString(this ITenantConfiguration config, string name)
```

resolves the `ConnectionStrings:{name}` entry from the current tenant configuration (tenant values merged with `MultiTenancy:Defaults`), either from separate connection info segments or from a standalone string:
  ```json
     "ConnectionStrings": {
        "MyDatabase": {
          "Server": "myserver,3342",
          "Database": "mydb",
          "UserName": "myuser",
          "Password": "mypass",
          "OtherParams": "MultipleActiveResultSets=true"
        }
  ```

It delegates to `IConfiguration.ResolveConnectionString` from [NBB.Core.Configuration](../../Core/NBB.Core.Configuration/README.md#connection-strings), which documents the supported segments, including identity-based authentication (`Authentication`, `IdentityClientId`). For passwordless modes such as `Active Directory Workload Identity`, `UserName` and `Password` are ignored, even when merged from `MultiTenancy:Defaults`, so a tenant can switch to a managed identity while the shared SQL login stays in the defaults:
  ```json
     "MultiTenancy": {
        "Defaults": {
          "ConnectionStrings": {
            "MyDatabase": { "UserName": "myuser", "Password": "mypass" }
          }
        },
        "Tenants": {
          "TenantA": {
            "ConnectionStrings": {
              "MyDatabase": {
                "Server": "myserver.database.windows.net",
                "Database": "tenant_a",
                "Authentication": "Active Directory Workload Identity",
                "IdentityClientId": "00000000-0000-0000-0000-000000000000"
              }
            }
          }
        }
     }
  ```

A missing mandatory segment throws `NBB.Core.Configuration.ConnectionStringConfigurationException`. The former `TenantConnectionStringBuilderException` is obsolete and no longer thrown.

The `GetValue<T>` extension reads values or complex objects from configuration:
```csharp
/// <summary>
    /// Extracts the value with the specified key and converts it to type T.
    /// or
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    public static T GetValue<T>(this ITenantConfiguration config, string key)
```csharp

### Service registration:
```csharp
services.AddDefaultTenantConfiguration();
```

### The default tenant configuration uses:
-  the `MultiTenancy` configuration section to read tenant specific configuration for the current tenant context, when `TenancyType` is set to `"MultiTenant"`
-  the default configuration root when `TenancyType` is set to `"MonoTenant"`

The `MultiTenancy:Defaults` section provides common tenant configuration that will be merged with tenant specific configurations.

