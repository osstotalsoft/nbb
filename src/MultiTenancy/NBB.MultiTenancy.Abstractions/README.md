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

reads connection string configuration from:
  - separate connection info segments:
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
  - or standalone string:
  ```json
     "ConnectionStrings": {
        "MyDatabase": "Server=myserver,3342;Database=mydb;User Id=myuser;Password=mypass;MultipleActiveResultSets=true"
  ```

The segmented form also accepts two optional keys for identity-based authentication:
  - `Authentication` - the SqlClient authentication mode. When it is missing or empty, the connection string is built exactly as above.
  - `IdentityClientId` - the client id of the managed identity, used as `User Id` for passwordless modes.

For the passwordless modes `Active Directory Workload Identity`, `Active Directory Managed Identity`, `Active Directory MSI` and `Active Directory Default` (matched case-insensitively and ignoring spaces, e.g. `ActiveDirectoryWorkloadIdentity`), `Server`, `Database` and `IdentityClientId` are mandatory, while `UserName` and `Password` are ignored (even when merged from `MultiTenancy:Defaults`):
  ```json
     "ConnectionStrings": {
        "MyDatabase": {
          "Server": "myserver.database.windows.net",
          "Database": "mydb",
          "Authentication": "Active Directory Workload Identity",
          "IdentityClientId": "00000000-0000-0000-0000-000000000000",
          "OtherParams": "MultipleActiveResultSets=true"
        }
  ```
  results in `Server=myserver.database.windows.net;Database=mydb;User Id=00000000-0000-0000-0000-000000000000;Authentication=Active Directory Workload Identity;MultipleActiveResultSets=true`

Any other `Authentication` value (e.g. `Active Directory Service Principal`) requires `UserName` and `Password` and is appended to the connection string:
`Server=...;Database=...;User Id=myuser;Password=mypass;Authentication=Active Directory Service Principal;...`

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

