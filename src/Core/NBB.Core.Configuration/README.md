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
