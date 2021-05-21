# The domain building blocks


> Domain-Driven Design: Tackling Complexity in the Heart of Software
>
> -- <cite>Eric Evans</cite>

All the domain building blocks are encapsulated in the package [`NBB.Domain`](./NBB.Domain#readme).

## Aggregates and Roots

>Aggregate is a pattern in Domain-Driven Design. A DDD aggregate is a cluster of domain objects that can be treated as a single unit. An example may be an order and its line-items, these will be separate objects, but it's useful to treat the order (together with its line items) as a single aggregate.
>An aggregate will have one of its component objects be the aggregate root. Any references from outside the aggregate should only go to the aggregate root. The root can thus ensure the integrity of the aggregate as a whole.
>Aggregates are the basic element of transfer of data storage - you request to load or save whole aggregates. Transactions should not cross aggregate boundaries.
>
> -- <cite>Martin Fowler</cite>

>Both Law of Demeter [Appleton, LoD] and Tell, Don’t Ask [PragProg, TDA] are design principles that can be used when implementing Aggregates, both of which stress information hiding.
>
> -- <cite>Vaughn Vernon</cite>


Rules of design:
* Aggregate roots have global identity, they  are created through factories, factory methods or constructors, hydrated and persisted through repositories as a whole. Only Aggregate Roots can be obtained directly with database queries. Everything else must be done through traversal.
* Keep aggregates small, as the aggregate is the transactional and consistency boundary. Big aggregates limit concurrency because of locking.
* The aggregate root is responsible for the whole aggregate invariants. The aggregate should be valid at any point in time.
* Aggregates should not reference other aggregates, they can reference other aggregates or entities identity.
* A delete operation must remove everything within the Aggregate boundary all at once
* Prefer asynchronous event handlers when sync-ing between aggregates.
* No lazy loading required
* Choose CQRS if you want a model freed up of query /read responsibilities.


[`NBB.Domain`](./NBB.Domain#readme) exposes two types of aggregate roots, both are evented in the sense that they record / store events to represent the state changes.
The difference between them is that, while with the *EventedAggregateRoot* you are free to alter state without emitting an event, with the *EventSourcedAggregateRoot* you cannot alter state without emitting an event.

### EventedAggregateRoot
The EventedAggregateRoot exposes the *AddEvent* witch allows you to publish events for observability reasons.

```csharp
public class Invoice : EventedAggregateRoot<Guid>
{
    public Guid InvoiceId { get; private set; }

    public Guid ClientId { get; private set; }

    public Guid? ContractId { get; private set; }

    public decimal Amount { get; private set; }

    public bool IsPayed => PaymentId.HasValue;

    public Guid? PaymentId { get; private set; }

    public Invoice(Guid clientId, Guid? contractId, decimal amount)
    {
        InvoiceId = Guid.NewGuid();
        Amount = amount;
        ClientId = clientId;
        ContractId = contractId;

        AddEvent(new InvoiceCreated(InvoiceId, amount, clientId, contractId));
    }

    public override Guid GetIdentityValue() => InvoiceId;

    public void MarkAsPayed(Guid paymentId, Guid? contractId)
    {
        PaymentId = paymentId;
        AddEvent(new InvoicePayed(InvoiceId, paymentId, contractId));
    }
}
```

### EventSourcedAggregateRoot
The EventSourcedAggregateRoot uses the emit-apply pattern, where emitting events is not just for observability but also for storing state changes. 
Side effects not recorded in the form of events will not be persisted.
The EventSourcedAggregateRoot forces you to obey the Bertrand Meyer's CQS principle, that states that every method should either be a command that performs an action, or a query that returns data to the caller, but not both. 
In other words, Asking a question should not change the answer.[1] More formally, methods should return a value only if they are referentially transparent and hence possess no side effects.

```csharp
public class Payable : EventSourcedAggregateRoot<Guid>
{
    public Guid PayableId { get; private set; }
    public Guid ClientId { get; private set; }
    public decimal Amount { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Payment Payment { get; private set; }
    public Guid? ContractId { get; private set; }
    public bool IsPayed => Payment != null;

    public Payable(Guid clientId, decimal amount, Guid? invoiceId, Guid? contractId)
    {
        Emit(new PayableCreated(Guid.NewGuid(), invoiceId, clientId, contractId, amount));
    }

    public override Guid GetIdentityValue() => PayableId;

    public void Pay()
    {
        if (IsPayed)
            throw new Exception("payment already payed");

        Emit(new PaymentReceived(Guid.NewGuid(), PayableId, InvoiceId, ContractId, DateTime.Now));
    }

    private void Apply(PayableCreated e)
    {
        PayableId = e.PayableId;
        ClientId = e.ClientId;
        Amount = e.Amount;
        InvoiceId = e.InvoiceId;
        ContractId = e.ContractId;
    }

    private void Apply(PaymentReceived e)
    {
        Payment = new Payment(e.PaymentId, e.PaymentDate, PayableId);
    }
}
```


Entities
----------------
> Many objects are not fundamentally defined by their attributes, but rather by a thread of continuity and identity.
>
> -- <cite>Eric Evans</cite>

>My “litmus test” for Entities is a simple question:
>If two instances of the same object have different attribute values, but same identity value, are they the same entity?
>If the answer is “yes”, and I care about an identity, then the class is indeed an entity. I model entities with reference objects (classes), and I give them a surrogate identity (i.e., probably a GUID). Additionally, my model must include what it means to have the same identity. That means overriding Equals, looking solely at the identity and not attributes.
>
> -- <cite>Jimmy Bogard</cite>

Entities have internal identity within an aggregate. The Entity base class provides identity based equality.
Entities can only be instantiated in the context of an aggregate root.
The aggregate root's *query* methods are not allowed to return entities. Follow the principle *Tell, Don’t Ask*.

```csharp
public class ContractLine : Entity<Guid>
{
    public Guid ContractLineId { get; private set; }
    public Product Product { get; private set; }
    public int Quantity { get; private set; }

    public Guid ContractId { get; private set; }


    internal ContractLine(Product product, int quantity, Guid contractId)
    {
        Product = product;
        Quantity = quantity;
        ContractId = contractId;
    }

    public override Guid GetIdentityValue() => ContractLineId;
}
```

Value objects
----------------
> Many objects have no conceptual identity. These objects describe characteristics of a thing.
>
> -- <cite>Eric Evans</cite>

> When I don’t care about some object’s identity, I carefully consider making the concept a value object.  For example, if I have a system that models Paint buckets, the Color is a great candidate for a Value Object. I care about one specific PaintBucket or another, as I paint with individual PaintBuckets that will eventually be drained of their paint.
>
> But when checking the Color of a specific PaintBucket, the Color has no identity in an of itself. If I have two Colors with the exact same pigmentation values, I consider them to be the same.
>
> When designing Value Objects, I want to keep them away from the trappings of Entity life cycles, so I make the Value Object immutable, and remove any concept of identity.  Additionally, I’ll override Equals to compare attributes, so that attribute equality is represented in my model.
>
> By making my Value Object immutable, many operations are greatly simplified, as I’m immediately led down paths to Side-Effect Free Functions. I don’t create a type with a bunch of read-write properties and call it a Value Object. I make it immutable, put all of the attributes in the constructor, and enforce attribute equality.
>
> Value Objects, like any other pattern, can be over-applied if you go hunting for opportunities. Value Objects should represent concepts in your Ubiquitous Language, and a domain expert should be able to recognize it in your model.
>
> -- <cite>Jimmy Bogard</cite>

Value objects have structural identity, are immutable, they don't have side-effects so it's easier to work with.
You can return value objects from aggregate root's *query* methods, you can instantiate value objects anywhere, domain events can contain them.

```csharp
public record VattedAmount(decimal Value, decimal VatValue, string Currency)
{
    public decimal TotalValue => Value + VatValue;
}

public record ContractId(Guid Value) : SingleValueObject<Guid>(Value);
```

Domain events
----------------
> History is the version of past events that people have decided to agree upon.
>
> -- <cite>Napoleon Bonaparte</cite>

> Model information about activity in the domain as a series of discrete events.
> Represent each event as a domain object. . . . A domain event is a full-fledged part of the domain model, a representation of something that happened in the domain.
>
> -- <cite>Eric Evans</cite>


> All events should be represented as verbs in the past tense such as CustomerRelocated, CargoShipped, or InventoryLossageRecorded. 
> For those who speak French, it should be Passé Composé, they are things that have completed in the past
>
> -- <cite>Greg Young</cite>

An event is something that has happened in the past, that the domain experts care about. 
A domain event is, logically, something that happened in a particular domain, and something you want other parts of the same domain (in-process) to be aware of and potentially react to.
An important benefit of domain events is that side effects after something happened in a domain can be expressed explicitly instead of implicitly. 
Those side effects must be consistent so either all the operations related to the business task happen, or none of them. In addition, domain events enable a better separation of concerns among classes within the same domain.

Domain events as a preferred way to trigger side effects across multiple aggregates within the same domain.

![domain events](/images/domain_events.png)


```csharp
public class ContractValidated : DomainEvent
{
    public Guid ContractId { get; }
    public Guid ClientId { get; }
    public decimal Amount { get; }

    [JsonConstructor]
    private ContractValidated(Guid eventId, DomainEventMetadata metadata,
        Guid contractId, Guid clientId, decimal amount)
        : base(eventId, metadata)
    {
        ContractId = contractId;
        ClientId = clientId;
        Amount = amount;
    }

    public ContractValidated(Guid contractId, Guid clientId, decimal amount)
        : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow }, contractId, clientId, amount)
    {
    }

}
```

Repositories
----------------
> For each type of object that needs global access, create an object that can provide the illusion of an in-memory collection of all objects of that type. 
> Set up access through a well-known global interface. Provide methods to add and remove objects. . . . 
> Provide methods that select objects based on some criteria and return fully instantiated objects or collections of objects whose attribute values meet the criteria. . . . 
> Provide repositories only for aggregates. . . .
>
> -- <cite>Eric Evans</cite>

Keep in mind that the shape of the repository is a domain concept, but the implementation is in the infrastructure layer.


For EventedAggregates we provide an EntityFramework implementation (NBB.Data.EntityFramework) for the generic repository *ICrudRepository*.
It's your decision to use this as a building block for your specific domain tuned repositories or use the generic built-in one.
If you want to use an EventStore for Audit you can hook the *EventedCrudRepositoryDecorator* that pushes the events in the EventStore.

```csharp
 public static class DependencyInjectionExtensions
    {
        public static void AddInvoicesDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services
                .AddScoped<ICrudRepository<Invoice>, EfCrudRepository<Invoice, InvoicesDbContext>>()
                .Decorate<ICrudRepository<Invoice>, EventedCrudRepositoryDecorator<Invoice>>();

            services.AddDbContext<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }
    }
```

For EventSourcedAggregates we provide a generic *EventSourcedRepository* that makes use of the default configured IEventStore.
```csharp
public static class DependencyInjectionExtensions
    {
        public static void AddContractsWriteModelDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IEventSourcedRepository<Domain.ContractAggregate.Contract>, EventSourcedRepository<Domain.ContractAggregate.Contract>>();
            services.AddScoped<IEventSourcedRepository<Domain.InsuranceAggregate.Insurance>, EventSourcedRepository<Domain.InsuranceAggregate.Insurance>>();
            services.AddScoped<IEventSourcedRepository<Domain.InvoiceAggregate.Invoice>, EventSourcedRepository<Domain.InvoiceAggregate.Invoice>>();
        }
    }
```

Domain services
----------------
> A Service in the domain is a stateless operation that fulfills a domain-specific task. 
> Often the best indication that you should create a Service in the domain model is when the operation you need to perform feels out of place as a method on an Aggregate (10) or a Value Object (6). 
> To alleviate that uncomfortable feeling, our natural tendency might be to create a static method on the class of an Aggregate Root. 
> However, when using DDD, that tactic is a code smell that likely indicates you need a Service instead.
>
> -- <cite>Vaughn Vernon</cite>


We don't provide any base classes for your domain services as there aren't any constraints.
Keep in mind that you should not abuse the concept, behavior should be placed in the aggregates as much as you can.