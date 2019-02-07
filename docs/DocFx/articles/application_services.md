The application services
===============

> The Application Services are the direct clients of the domain model.
> ...
> These are responsible for task coordination of use case flows, one service method per flow. 
> When using an ACID database, the Application Services also control transactions, ensuring that model state transitions are atomically persisted. 
> Security is also commonly cared for by Application Services.
> It is a mistake to consider Application Services to be the same as Domain Services (7). They are not.
> ...
> We should strive to push all business domain logic into the domain model, whether that be in Aggregates, Value Objects, or Domain Services. 
> Keep Application Services thin, using them only to coordinate tasks on the model.
>
> -- <cite>Vaughn Vernon</cite>


All the application building blocks are encapsulated in the package *NBB.Application*.


This is the direct client of the business model.
The main responsability here is providing the use-cases of the application.
You are not allowed to return business objects out of this layer so you will need some kind of DTO's to return to your clients. That's were Commands and Queries come in to play.
If you design your application to be CQRS you may not need Queries as you can expose Read Model Repositories if the read model entities are serializable.

For dispatching commands, queries and events we use Jimmy Bogard's mediator implementation [MediatR](https://github.com/jbogard/MediatR).

![application-services](/images/application-services.png)

Commands
----------------

```csharp
public class ValidateContract : IntegrationCommand, IKeyProvider
{
    public Guid ContractId { get; }

    string IKeyProvider.Key => ContractId.ToString();

    [JsonConstructor]
    private ValidateContract(Guid contractId, Guid commandId, ApplicationMetadata metadata)
        : base(commandId, metadata)
    {
        ContractId = contractId;
    }
}
```

Command handlers
----------------

Just implement the interface IRequestHandler<TCommand> and the system will automatically hook your command handler.

```csharp
public class ContractCommandHandlers :
        IRequestHandler<CreateContract>
    {
        private readonly IEventSourcedRepository<Contract> _repository;

        public ContractCommandHandlers(IEventSourcedRepository<Contract> repository
        {
            this._repository = repository;
        }

        public async Task Handle(CreateContract command, CancellationToken cancellationToken)
        {
            var contract = new Contract(command.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);
        }
    }
```

Events
----------------
There are two types of events: Domain events and Integration events

### Domain Events

```csharp
public class InvoiceCreated : DomainEvent
{
    public Guid InvoiceId { get; }

    public decimal Amount { get; }

    public Guid ClientId { get; }

    public Guid? ContractId { get; }


    [JsonConstructor]
    private InvoiceCreated(Guid eventId, DomainEventMetadata metadata, Guid invoiceId, decimal amount, Guid clientId, Guid? contractId) 
        : base(eventId, metadata)
    {
        InvoiceId = invoiceId;
        Amount = amount;
        ClientId = clientId;
        ContractId = contractId;
    }

    public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId)
        : this(Guid.NewGuid(), new DomainEventMetadata {CreationDate = DateTime.UtcNow, SequenceNumber = 0, CorrelationId = null }, 
        invoiceId, amount, clientId, contractId)
    {
    }
}
```

### Integration Events

```csharp
public class InvoiceCreated : IntegrationEvent
{
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? ContractId { get; private set; }


    [JsonConstructor]
    private InvoiceCreated(Guid eventId, ApplicationMetadata metadata,
        Guid invoiceId, decimal amount, Guid clientId, Guid? contractId)
        : base(eventId, metadata)
    {
        InvoiceId = invoiceId;
        Amount = amount;
        ClientId = clientId;
        ContractId = contractId;
    }

    public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId, Guid? correlationId)
        : this(Guid.NewGuid(), new ApplicationMetadata { CreationDate = DateTime.UtcNow, CorrelationId = correlationId }, 
            invoiceId, amount, clientId, contractId)
    {
    }
}
```


Event handlers
----------------
Just implement the interface INotification<TEvent> and the system will automatically hook your event handler.


```csharp
public class ContractReadModelGenerator :
        INotificationHandler<Domain.ContractAggregate.DomainEvents.FinancedAssetAdded>
    {
        private readonly IMapper _mapper;
        private readonly ICrudRepository<ReadModel.Contracts.FinancedAsset> _financedAssetReadModelRepository;

        public ContractReadModelGenerator(IMapper mapper, ICrudRepository<ReadModel.Contracts.FinancedAsset> financedAssetReadModelRepository)
        {
            _mapper = mapper;
            _financedAssetReadModelRepository = financedAssetReadModelRepository;
        }

        public async Task Handle(Domain.ContractAggregate.DomainEvents.FinancedAssetAdded @event, CancellationToken cancellationToken)
        {


            var financedAssetExists = await _financedAssetReadModelRepository.AnyAsync(fa => fa.FinancedAssetId == @event.FinancedAssetId);
            if (financedAssetExists)
            {
                //TODO: dedup
                return;
            }

            var financedAsset = _mapper.Map<ReadModel.Contracts.FinancedAsset>(@event);
            await _financedAssetReadModelRepository.AddAsync(financedAsset);
            await _financedAssetReadModelRepository.SaveChangesAsync();
        }

    }
```