The repositories
===============

In Fowler’s PoEAA, the Repository pattern is described as:
> Mediates between the domain and data mapping layers using a collection-like interface for accessing domain objects.


 CRUD Repositories
----------------
Package *NBB.Data.EntityFramework* exposes EfCrudRepository, witch is a generic repository based on EF Core and implements ICrudRepository.
The use of generic repository is controversal in the DDD landscape, they say that you should only use single purpose repositories as dictated by the business domain.

You have to explicitly register the generic repository for a specific entity with the container:

```csharp
services.AddScoped<ICrudRepository<Payable>, EfCrudRepository<Payable, PaymentsDbContext>>();
```

Example usages:
```csharp
public async Task Handle(EventStoreEnvelope<ContractLineAdded> @event, CancellationToken cancellationToken)
{
    var de = @event.Event;
    var e = await _contractReadModelRepository.GetFirstOrDefaultAsync(x => x.ContractId == de.ContractId, "ContractLines");


    if (e != null)
    {
        if (e.ContractLines.All(cl => cl.ContractLineId != de.ContractLineId))
        {
            var contractLine = new ContractLineReadModel(de.ContractLineId, de.Product, de.Price, de.Quantity, de.ContractId);
            e.ContractLines.Add(contractLine);
            e.Version = de.SequenceNumber;

            await _contractReadModelRepository.SaveChangesAsync();
        }
    }
}
```

The package *NBB.Data.EventStoreExtensions* contains *EventStoreCrudRepositoryDecorator* witch is a decorator over ICrudRepository that intercepts *SaveChanges* method 
and if the entity is evented it pushes the events on the event store. It is suitable for evented write-models.
```csharp
services
    .AddScoped<ICrudRepository<Invoice>, EfCrudRepository<Invoice, InvoicesDbContext>>()
    .Decorate<ICrudRepository<Invoice>, EventedCrudRepositoryDecorator<Invoice>>();
```

Note that you should also provide an EventStore for this decorator to work.
```csharp
services.AddEventStore(o =>
{
    o.AddAdoNetEventRepository();
});
```

Read-Only Repositories
----------------
For read models it is a best practice to provide read-only repositories, although you will definetly need a CRUD one for read model generators.
```cshap
services.AddScoped<IReadOnlyRepository<Contract>, EfReadOnlyRepository<Contract, ContractsReadDbContext>>();
services.AddScoped<ICrudRepository<Contract>, EfCrudRepository<Contract, ContractsReadDbContext>>();
```

EventSourced Repositories
----------------
The package *NBB.Data.EventSourcing* provides the EventSourcedRepository usefull when doing event sourcing. It uses the configured EventStore.
EventSourcedRepository offers two operations:
```csharp
public interface IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot, new()
{
    Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default(CancellationToken));
    Task<TAggregateRoot> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken));
}
```
