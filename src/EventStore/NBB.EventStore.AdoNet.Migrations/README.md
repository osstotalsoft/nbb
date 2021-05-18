# NBB.Eventstore.AdoNet.Migrations

Creates the database objects for the [`NBB.EventStore.AdoNet`](../NBB.EventStore.AdoNet#readme) and [`NBB.EventStore.AdoNet.MultiTenancy`](../NBB.EventStore.AdoNet.MultiTenancy#readme) event repositories.

## Configuration

The configuration alows specifying:
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


