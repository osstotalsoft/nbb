# NBB.ProjectR

Functional style read-model projections inspired by [Elm](https://guide.elm-lang.org/)

## Motivation
CQRS is a great way of dealing with complex business domains, by separating the write-side from read-side, one can implement the write-model using tactical patterns from DDD and ES.
But also, this creates yet another problem, dealing with the read-model and data access.

I usually see two patterns of dealing with the read-side:
 - use read-model projections built from write-model events
 - create a database view built from write-model tables

The first one is the most expensive one but it leverages all the advantages promised by CQRS, while the second one is cheaper but does not solve any performance or availability problems.

ProjectR is meant to fill this need of implementing the read-model projections, in a simple way while leveraging all the advantages promised by CQRS

## NuGet install
```
dotnet add package NBB.ProjectR
```

## Creating projections
When creating projections, one has to provide the following information:
 - model - it can be of any type (interface, class, record), usually some record
    ```csharp
    public record Model(Guid ContractId, decimal Value, bool IsValidated = false,
        Guid? ValidatedByUserId = null, string ValidatedByUsername = null, bool IsSigned = false);
    ```
 - message - this will be projected, by the projector into the projection model, it can be of any type (interface, class, record), usually some record hierarchy simulating sum types
    ```csharp
    public record Message
    {
        public record CreateContract(Guid ContractId, decimal Value) : Message;

        public record ValidateContract(Guid ContractId, Guid UserId) : Message;

        public record SignContract(Guid ContractId, Guid SignerId) : Message;

        public record SetUserName(Guid ContractId, string Username) : Message;
    }
    ```
 - projector this component has two responsibilities:
   - reduce messages to the projection model
   - map subscription data to messages for some projection identity
    ```csharp
    class Projector: IProjector<Model, Message, Guid>
    {
        public (Model Model, Effect<Message> Effect) Project(Message message, Model model)
            => (message, model) switch
            {
                (Message.CreateContract msg, null) => (
                    new(msg.ContractId, msg.Value),
                    MessageBus.Publish(new ContractProjectionCreated(msg.ContractId, msg.Value))
                        .Then(Eff.None<Message>())),

                (Message.ValidateContract msg, { IsValidated: false }) => (
                    model with { IsValidated = true, ValidatedByUserId = msg.UserId },
                    Mediator.Send(new LoadUserById.Query(msg.UserId)).Then(x =>
                        Eff.OfMsg<Message>(new Message.SetUserName(msg.ContractId, x.UserName)))),

                (Message.SetUserName msg, not null) => (
                    model with { ValidatedByUsername = msg.Username },
                    MessageBus.Publish(new ContractProjectionValidated(model.ContractId, model.ValidatedByUserId, msg.Username))
                        .Then(Eff.None<Message>())),

                (Message.SignContract, not null) => (model with { IsSigned = true }, Eff.None<Message>()),

                _ => (model, Eff.None<Message>())
            };

        public (Guid Identity, Message Message) Subscribe(INotification @event) => @event switch
        {
            ContractCreated ev => (ev.ContractId, new Message.CreateContract(ev.ContractId, ev.Value)),
            ContractValidated ev => (ev.ContractId, new Message.ValidateContract(ev.ContractId, ev.UserId)),
            ContractSigned ev => (ev.ContractId, new Message.SignContract(ev.ContractId, ev.SignerId)),
            _ => (default, default)
        };
    }
    ```
 - metadata - this provides info for infrastructure related concerns like:
   - messaging subscriptions
   - es snapshot frequency
    ```csharp
    [SnapshotFrequency(2)]
    class Projector: 
        IProjector<Model, Message, Guid>,
        ISubscribeTo<ContractCreated, ContractValidated, ContractSigned>
    ```

## Service registration
This library depends on:
   - NBB effect system
   - NBB EventStore
   - MediatR

so you will need to register the dependent services, like so:
```csharp
services.AddProjectR(GetType().Assembly);
services.AddMediatR(GetType().Assembly);
services
    .AddEffects()
    .AddMessagingEffects()
    .AddMediatorEffects();
services.AddMessageBus().AddInProcessTransport();
services.AddEventStore()
    .WithNewtownsoftJsonEventStoreSeserializer()
    .WithInMemoryEventRepository();
```

## Loading projections
For storing projections, the library uses ES based on projection messages, this means that you can only load projections by their identity. For this matter the library exposes the following abstraction:
```csharp
public interface IReadModelStore<TModel>
{
    Task<TModel> Load(object id, CancellationToken cancellationToken);
}
```