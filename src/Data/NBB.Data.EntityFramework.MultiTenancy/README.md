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
You need to provide an implementation for `ITenantDatabaseConfigService` so the system can find the database settings for the current tenant.
```csharp
public interface ITenantDatabaseConfigService
{
    string GetConnectionString();
}
```

NBB provides an implementation that reads the connection info from the application's configuration (appsettings.json). It can be registerd as follows:
```csharp
services.AddTenantDatabaseConfigService<ConfigurationDatabaseConfigService>();
```

Multi-tenant DbContext configuration
----------------
* to configure a multi-tenant entity, you need to call `IsMultiTenant()` inside  entity configuration, like so:

```csharp
public class MyEntityConfiguration: IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder
            .ToTable("MyEntities")
            .IsMultiTenant()
            .HasKey(x => x.MyEntityId);
    }
}
```

* you can derive from `MultiTenantDbContext` in order to use multi-tenant capabilities:
```csharp
public class MyDbContext : MultiTenantDbContext
{
    public DbSet<MyEntity> MyEntities { get; set; }
    public TodoDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MyEntityConfiguration());
            
        base.OnModelCreating(modelBuilder);
    }
}
```
* if you cannot derive from MultiTenantDbContext, you can add multi-tenant capabilities to your existing DbContext by overriding the methods `SaveChangesAsync` and `SaveChanges` , like so:
```csharp

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
When registering multi-tenant DbContext you can use the `ITenantDatabaseConfigService` abstractions in order to get the tenant's connection string. You also need to call the `UseMultitenancy` extension:
```csharp
services.AddDbContext<KycDbContext>((serviceProvider, options) =>
{
    var databaseService = serviceProvider.GetRequiredService<ITenantDatabaseConfigService>();
    var connectionString = databaseService.GetConnectionString();
    
    options
        .UseSqlServer(connectionString)
        .UseMultitenancy(serviceProvider);
});
```

> Be aware that when using a multi-tenant dbContext, you cannot use the `DbContextPool`




