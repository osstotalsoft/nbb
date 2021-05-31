NBB.Data.EntityFramework.MultiTenancy
===============

The package *NBB.Data.EntityFramework.MultiTenancy* helps implementing a multi-tenant data access solution using Entity Framework.

NuGet install
----------------
```
dotnet add package NBB.Data.EntityFramework.MultiTenancy
```

Philosophy
----------------
When it comes to multi-tenant data access, there are 3 strategies:
* Database per tenant: in this case, the only thing you have to do, is to provide a connection string per tenant, when registering a DbContext. This strategy offers the biggest isolation level, but is the most expensive one
* Shared database: when using this strategy, multi-tenant tables are partitioned by a TenantId column. This package provides extensions for EF objects, in order to configure both a shadow state property `TenantId` and a global query filter with the value set to `ITenantContext.GetTenantId`, so that user queries can remain tenancy agnostic. This strategy offers a poor isolation level so it must be used with caution!

* A mix of shared and dedicated databases: some tenants may be shared in the same database while others have dedicated database instances

This package supports all 3 strategies stated above, so that you can write tenancy ignorant queries


Tenant database configuration
----------------
You need to provide an implementation for `ITenantDatabaseConfigService` so that when executing a query, the system knows to inject or not a TenantId filter.
```csharp
public interface ITenantDatabaseConfigService
{
    string GetConnectionString(Guid tenantId);
    bool IsSharedDatabase(Guid tenantId);
}
```

Multi-tenant DbContext configuration
----------------
* to configure a multi-tenant entity, you need to call `ApplyMultiTenantConfiguration` inside  `OnModelCreating`, like so:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyMultiTenantConfiguration(new AgentConfiguration(), this);
    modelBuilder.ApplyMultiTenantConfiguration(new CustomerConfiguration(), this);
    modelBuilder.ApplyMultiTenantConfiguration(new DocumentConfiguration(), this);
    modelBuilder.ApplyMultiTenantConfiguration(new IdentificationConfiguration(), this);
    modelBuilder.ApplyMultiTenantConfiguration(new IdentificationStatisticConfiguration(), this);
}
```
* if you use unit of work repositories register the `MultiTenantEfUow` by calling `AddMultiTenantEfUow` or `AddMultiTenantEfCrudRepository` in the composition root:
```csharp
services.AddMultiTenantEfUow<MyEntity, MyDbContext>();
```
* if you do not use the unit of work for saving entities, you need to override the methods `SaveChangesAsync` and `SaveChanges` in every multi-tenant dbContext, like so:
```csharp
public MyDbContext(DbContextOptions<KycDbContext> options) : base(options)
{
}

public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
{
    this.SetTenantIdFromContext();
   
    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
}

public override int SaveChanges(bool acceptAllChangesOnSuccess)
{
    this.SetTenantIdFromContext();

    return base.SaveChanges(acceptAllChangesOnSuccess);
}
```

DbContext container registration
----------------
When registering multi-tenant DbContext you can use the `ITenantContextAccessor` and `ITenantDatabaseConfigService` abstractions in order to get the tenant's connection string.
```csharp
services.AddDbContext<KycDbContext>((serviceProvider, options) =>
{
    var databaseService = serviceProvider.GetService<ITenantDatabaseConfigService>();
    var accessor = serviceProvider.GetService<ITenantContextAccessor>();
    var tenantId = accessor.TenantContext.GetTenantId();
    var connectionString = databaseService.GetConnectionString(tenantId);
    
    options.UseSqlServer(connectionString);
```

> Be aware that when using a multi-tenant dbContext, you cannot use the `DbContextPool`




