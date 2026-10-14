# NBB.Core.Configuration

This package contains configuration extensions.

## NuGet install
```
dotnet add package NBB.Core.Configuration
```

## DotEnv configuration provider
Adds support for configuration files that contain environment variable declarations.

### Registration
```csharp
var builder = new ConfigurationBuilder();
builder.AddDotEnvFile("config/env.txt", optional: false, reloadOnChange: true);
```

Parameters:
* `path` - The path of the configuration file relative to the base path.
* `optional` - Whether the file is optional.
* `reloadOnChange` - Whether the configuration should be reloaded if the file changes.

### Config file example
```ini
# Serilog
Serilog__MinimumLevel__Default             = Information
Serilog__MinimumLevel__Override__Microsoft = Warning

# MultiTenancy
MultiTenancy__Enabled = true
MultiTenancy__Tenants__BCR__ConnectionStrings__App_Database__User_Name = TestUser
MultiTenancy__Tenants__BCR__Description = "My description  "
```

Notes:
* Each line should contain the key and the value separated by character `=`
* For hierarchical settings, the "__" separator should be used.
* Comments can be added in the file by starting the line with character `#`.
* Empty lines and whitespace around keys/values are ignored.
* Values that start or end with whitespace should be surounded by quotes. The quotes will not be part of the value.

## Connection strings
The `ResolveConnectionString` extension builds a connection string from configuration, so that its parts can come from different sources (e.g. server and database from a config map, credentials from a secret, the authentication mode from an environment variable):
```csharp
string? connectionString = configuration.ResolveConnectionString("MyDatabase");
```

It reads the `ConnectionStrings:{name}` entry, either as a standalone string:
```json
"ConnectionStrings": {
  "MyDatabase": "Server=myserver,3342;Database=mydb;User Id=myuser;Password=mypass;MultipleActiveResultSets=true"
}
```
or from separate connection info segments:
```json
"ConnectionStrings": {
  "MyDatabase": {
    "Server": "myserver,3342",
    "Database": "mydb",
    "UserName": "myuser",
    "Password": "mypass",
    "OtherParams": "MultipleActiveResultSets=true"
  }
}
```
which results in `Server=myserver,3342;Database=mydb;User Id=myuser;Password=mypass;MultipleActiveResultSets=true`. When both forms are present, the segments win. `null` is returned when the entry is missing.

Segments:
* `Server`, `Database` - mandatory.
* `UserName`, `Password` - mandatory, except for passwordless authentication modes.
* `OtherParams` - optional, appended as is.
* `Authentication` - optional SqlClient authentication mode. When it is missing or empty, the connection string is built as above.
* `IdentityClientId` - the client id of the managed identity, used as `User Id` for passwordless authentication modes.

For the passwordless modes `Active Directory Workload Identity`, `Active Directory Managed Identity`, `Active Directory MSI` and `Active Directory Default` (matched case-insensitively and ignoring spaces, e.g. `ActiveDirectoryWorkloadIdentity`), `IdentityClientId` is mandatory and `UserName` / `Password` are ignored:
```json
"ConnectionStrings": {
  "MyDatabase": {
    "Server": "myserver.database.windows.net",
    "Database": "mydb",
    "Authentication": "Active Directory Workload Identity",
    "IdentityClientId": "00000000-0000-0000-0000-000000000000",
    "OtherParams": "MultipleActiveResultSets=true"
  }
}
```
results in `Server=myserver.database.windows.net;Database=mydb;User Id=00000000-0000-0000-0000-000000000000;Authentication=Active Directory Workload Identity;MultipleActiveResultSets=true`.

Any other `Authentication` value (e.g. `Active Directory Service Principal`) requires `UserName` and `Password` and is appended after them:
`Server=...;Database=...;User Id=myuser;Password=mypass;Authentication=Active Directory Service Principal;...`

A missing mandatory segment throws `ConnectionStringConfigurationException` (namespace `NBB.Core.Configuration`).

Notes:
* The method is named `ResolveConnectionString` to avoid clashing with `Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString`, which only reads string values.
* Components that call `GetConnectionString` internally (e.g. Aspire client integrations) do not see segmented connection strings; pass the resolved string to them explicitly.
