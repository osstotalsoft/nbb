The read models
===============

> CQRS is an architectural style that is often enabling of DDD.
>
> Eric Evans, tweet February 2012.


![cqrs](/images/cqrs.png)


When doing CQRS, it is common to subscribe to write-model events and build one or more read-model projections. 
Theese projections can be denormalized in order to support the views data access without the need of joins resulting in high performance queries.
Another benefit of CQRS is that it does not block the queries while the locks are beeing yielded on the write-model store.

The added complexity lies in keeping read-models in sync with the write-models and adopting the eventual consistency.

Read model entities
----------------
There is no guidence in CH-BB regarding the shape of your read-models, so you will freely design the POCO's, in generally anemic.

Example:
```csharp
public class Insurance
{
    public long InsuranceId { get; set; }
    public long ContractId { get; set; }
    public long? FinancedAssetId { get; set; }
    public long? PaymentScheduleLineId { get; set; }
    public string InsuranceType { get; set; }
    public string InsuranceNumber { get; set; }
    public string InsuranceCompany { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PremiumsCount { get; set; }
    public string Currency { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal InsuredValue { get; set; }
    public string Status { get; set; }
}
```

Read model repositories
----------------

You will often provide a CRUD and a ReadOnly repository for each read-model projection, you can use the generic ones or provide some custom tailored ones if you feel so.
```csharp
services.AddScoped<IReadOnlyRepository<Insurance>, EfReadOnlyRepository<Insurance, ContractsReadDbContext>>();
services.AddScoped<ICrudRepository<Insurance>, EfCrudRepository<Insurance, ContractsReadDbContext>>();
```

Read model generators
----------------

You will handle domain events in order to mantain your read-models.
You have two options:
1. Synchronous domain event handlers (in process)
```csharp
public class InvoiceReadModelGenerator :
        INotificationHandler<Domain.InvoiceAggregate.DomainEvents.InvoiceCreated>
{
    private readonly IMapper _mapper;
    private readonly ICrudRepository<ReadModel.Invoices.Invoice> _invoiceReadModelRepository;

    public InvoiceReadModelGenerator(IMapper mapper,
        ICrudRepository<ReadModel.Invoices.Invoice> invoiceReadModelRepository)
    {
        _mapper = mapper;
        _invoiceReadModelRepository = invoiceReadModelRepository;
    }

    public async Task Handle(Domain.InvoiceAggregate.DomainEvents.InvoiceCreated @event, CancellationToken cancellationToken)
    {

        var invoceExists = await _invoiceReadModelRepository.AnyAsync(c => c.InvoiceId == @event.InvoiceId);
        if (invoceExists)
        {
            //TODO: dedup
            return;
        }

        var invoice = _mapper.Map<ReadModel.Invoices.Invoice>(@event);
        await _invoiceReadModelRepository.AddAsync(invoice);
        await _invoiceReadModelRepository.SaveChangesAsync();
    }
}
```

2. Asynchronous event store stream event handlers:
![event-store-stream-event-handler](/images/event-store-stream-event-handler.jpeg)
```csharp
public class ReadModelGenerator :
    INotificationHandler<EventStoreEnvelope<ContractCreated>>
{
    private readonly ICrudRepository<ContractReadModel> _contractReadModelRepository;

    public ReadModelGenerator(ICrudRepository<ContractReadModel> contractReadModelRepository)
    {
        _contractReadModelRepository = contractReadModelRepository;
    }

    public async Task Handle(EventStoreEnvelope<ContractCreated> @event, CancellationToken cancellationToken)
    {
        var de = @event.Event;
        var c = await _contractReadModelRepository.GetFirstOrDefaultAsync(x=> x.ContractId == de.ContractId);
        if (c == null)
        {
            await _contractReadModelRepository.AddAsync(
                new ContractReadModel(de.ContractId, de.ClientId, de.SequenceNumber));
            await _contractReadModelRepository.SaveChangesAsync();
        }
    }
}
```
