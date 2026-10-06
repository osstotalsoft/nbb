# NBB.EventStore.AdoNet.Migrations

Creates the database objects for the [`NBB.EventStore.AdoNet`](../NBB.EventStore.AdoNet#readme) and [`NBB.EventStore.AdoNet.MultiTenancy`](../NBB.EventStore.AdoNet.MultiTenancy#readme) event repositories.

## Configuration

The configuration allows specifying:
* the connection string of the event store database
* the tenancy type (determines whether to add multi-tenant support)

```json
{
  "EventStore": {
    "NBB": {
      "ConnectionString": "Server=YOUR_SERVER;Database=EventStore;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true"
    }
  },
  "MultiTenancy": {
    "TenancyType": "None" // "MultiTenant" "MonoTenant"
  }
}

```

## Usage

By default the migrator reads the configuration itself, from `appsettings.json`, environment variables and (in Development) user secrets:

```csharp
await new AdoNetEventStoreDatabaseMigrator().CreateDatabaseObjectsAsync();
```

To use the configuration of an existing host instead, for example one built with `Host.CreateApplicationBuilder`, pass its `IConfiguration`:

```csharp
await new AdoNetEventStoreDatabaseMigrator(builder.Configuration).CreateDatabaseObjectsAsync();
```
