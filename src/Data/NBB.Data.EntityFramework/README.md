NBB.Data.EntityFramework
===============

The package *NBB.Data.EntityFramework* provides some implementations formalized in *NBB.Data.Abstractions* using Entity Framework:
* `EfCrudRepository`
* `EfReadOnlyRepository`
* `EfUow` - unit of work implementation
* `EfQuery` - implementation of `IQueryable<TEntity>` and `IAsyncEnumerable<TEntity>`

NuGet install
----------------
```
dotnet add package NBB.Data.EntityFramework
```

CRUD Repositories
----------------
Package *NBB.Data.EntityFramework* exposes EfCrudRepository, witch is a generic repository based on EF Core and implements ICrudRepository.
The use of generic repository is controversal in the DDD landscape, they say that you should only use single purpose repositories as dictated by the business domain.

You have to explicitly register the generic repository for a specific entity with the container:

```csharp
services.AddEfCrudRepository<Payable, PaymentsDbContext>();
```

Example usages:
```csharp
public async Task Handle(EventStoreEnvelope<ContractLineAdded> @event, CancellationToken cancellationToken)
{
    var de = @event.Event;
    var e = await _contractReadModelRepository.GetByIdAsync(de.ContractId, cancellationToken, "ContractLines");


    if (e != null)
    {
        if (e.ContractLines.All(cl => cl.ContractLineId != de.ContractLineId))
        {
            var contractLine = new ContractLineReadModel(de.ContractLineId, de.Product, de.Price, de.Quantity, de.ContractId);
            e.ContractLines.Add(contractLine);

            await _contractReadModelRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
```

Read-Only Repositories
----------------
For read models, if you need a clean separation, you can  provide read-only repositories, although you may need a CRUD one for read model generators.
```cshap
services.AddScoped<IReadOnlyRepository<Contract>, EfReadOnlyRepository<Contract, ContractsReadDbContext>>();
services.AddScoped<ICrudRepository<Contract>, EfCrudRepository<Contract, ContractsReadDbContext>>();
```

Using Queries for read model
----------------
When writting read-model queries, you may not want to add another layer of abstraction, such as repositories, but still, you may not want to work directly with EF objects  in your Application Layer (Query handlers). If this is your case, you can choose to work with IQueriable<TEntity> and this is exactly what this package facilitates.

```csharp
services.AddEfQuery<Invoice, InvoicesDbContext>();
```

After this registration, you may inject `IQueryable<Invoice>` in your query handler or directly in the controller:
```csharp
 [Route("api/[controller]")]
    public class InvoicesController : Controller
    {
        private readonly IQueryable<Invoice> _invoiceQuery;

        public InvoicesController(IQueryable<Invoice> invoiceQuery)
        {
            _invoiceQuery = invoiceQuery;
        }


        // GET api/invoices
        [HttpGet]
        public Task<List<Invoice>> Get()
        {
            return _invoiceQuery.ToListAsync();
        }
```



Unit of work
----------------
This package offers `EfUow<TEntity, TContext>` as an implementation for the `IUow<TEntity>` from the abstractions package. This default implementation only delegates the `SaveChangesAsync` to the underlying DbContext.

When working with unit of work repositories you need to register the unit of work `EfUow`, like so:
```csharp
AddEfUow<TEntity, TContext>(this IServiceCollection services)
```

Registering an `EfCrudRepository` with `AddEfCrudRepository` will auto register a unit of work.


Internal services registration
----------------
You need to call `AddEntityFrameworkDataAccess` somewhere in the composition root, that registers some internal services, like so:
```csharp
AddEntityFrameworkDataAccess(this IServiceCollection services)
```