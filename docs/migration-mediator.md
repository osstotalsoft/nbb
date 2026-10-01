# Mediator adapters: migration guide (NBB 10.x)

Starting with this release the general-purpose NBB packages no longer depend on [MediatR](https://github.com/jbogard/MediatR).
All mediator-specific code lives in adapter packages, available for MediatR (`*.MediatR`) and for the source generated
[Mediator](https://github.com/martinothamar/Mediator) library (`*.Mediator`), which is the recommended one.

The release is a 10.x minor but it contains **breaking changes**: every application must add the adapter packages for the
features it uses, even when it stays on MediatR. Moved types keep their namespaces, so for MediatR applications the change is
mostly package references and a few registrations.

## Packages

| Feature | Library-free core | MediatR adapter | Mediator adapter |
|---|---|---|---|
| Messaging host: subscriber discovery, dispatch middleware | `NBB.Messaging.Host` | `NBB.Messaging.MediatR` | `NBB.Messaging.Mediator` |
| Application ports: in-process event publishing, contract classification (tenancy, NBB 4 topics) | `NBB.Core.Abstractions` (`IEventPublisher`, `IContractKindClassifier`), `NBB.Data.Abstractions` (`EventPublishingUowDecorator<>`) | `NBB.Application.MediatR` (`AddMediatRIntegration()`) | `NBB.Application.Mediator` (`AddMediatorIntegration()`) |
| Effects | `NBB.Core.Effects` | `NBB.Application.MediatR.Effects` | `NBB.Application.Mediator.Effects` |
| Process manager | `NBB.ProcessManager.Definition`, `NBB.ProcessManager.Runtime` | `NBB.ProcessManager.MediatR` | `NBB.ProcessManager.Mediator` |
| ProjectR | `NBB.ProjectR` | `NBB.ProjectR.MediatR` | `NBB.ProjectR.Mediator` |

No longer depending on MediatR: `NBB.Messaging.Host`, `NBB.Messaging.MultiTenancy`, `NBB.Messaging.BackwardCompatibility`,
`NBB.Data.EventSourcing`, `NBB.ProcessManager.Definition`, `NBB.ProcessManager.Runtime`, `NBB.ProjectR`,
`NBB.Application.DataContracts.Schema`, `NBB.Http.Effects`, `NBB.MultiTenancy.Identification.Messaging`.
`NBB.Application.DataContracts` (legacy NBB 4 contracts) still depends on `MediatR.Contracts`.

## Staying on MediatR

1. **Application ports** - reference `NBB.Application.MediatR` and register the MediatR implementations of the NBB application ports:
   ```csharp
   services.AddMediatRIntegration(); // IEventPublisher + IContractKindClassifier
   ```
   * `EventSourcedRepository<T>` now requires an `IEventPublisher` (resolving it fails otherwise). To publish nothing in-process, register your own `IEventPublisher` implementation instead.
   * The NBB 4 topic resolution (`UseTopicResolutionBackwardCompatibility`) and the messaging `TenantMiddleware` require an `IContractKindClassifier`: without it the host fails at start (NBB 4 topics) or the `TenantMiddleware` cannot be activated.
2. **Messaging host** - reference `NBB.Messaging.MediatR`. `FromMediatRHandled*()` and `UseMediatRMiddleware()` are unchanged.
3. **Unit of work publishing** - `MediatorUowDecorator<>` is obsolete (removed in NBB 11). Replace it with:
   ```csharp
   services.AddMediatRIntegration();
   services.Decorate(typeof(IUow<>), typeof(EventPublishingUowDecorator<>)); // NBB.Data.Abstractions
   ```
4. **Effects** - `AddMediatorEffects()` and `Mediator.Send/Publish` are obsolete aliases (removed in NBB 11) of `AddMediatREffects()` and `MediatorEff.Send/Publish`.
   `NBB.Application.Mediator.Effects` has the same `MediatorEff.Send/Publish` API, so effect code only changes its `using` when moving to Mediator.
5. **Process manager** - reference `NBB.ProcessManager.MediatR` and register the handlers after the process manager:
   ```csharp
   services
       .AddProcessManager(assemblies)
       .AddProcessManagerMediatRHandlers(); // also registers the MediatR effects
   ```
   `AddNotificationHandlers(Type)` was replaced by the library-free `GetProcessManagerEventRegistrations()`.
   `AddProcessManager()` and `AddProcessManagerDefinition()` now return `IServiceCollection` (were `void`), so registrations can be chained; recompile callers.
   The public helper class `NBB.ProcessManager.Definition.Preconditions` was removed (use `ArgumentNullException.ThrowIfNull`), together with the `JetBrains.Annotations` package dependency.
   `TimeoutOccured` and its handler were removed: due timeouts are published directly to the message bus.
6. **ProjectR** - reference `NBB.ProjectR.MediatR`, chain `.AddProjectRMediatRHandlers()` after `AddProjectR()`,
   and change projectors' `Subscribe(INotification @event)` to `Subscribe(object @event)`.
   Projectors using MediatR effects need `NBB.Application.MediatR.Effects` (no longer referenced by `NBB.ProjectR`).
7. `SchemaDefinitionUpdated` is a plain record (no longer a MediatR notification).

## Moving to Mediator

### Messages and handlers
* `MediatR` / `MediatR.Contracts` → `Mediator.Abstractions` in contracts and application projects; `using MediatR;` → `using Mediator;`.
* Handlers return `ValueTask`: `ValueTask` for notification handlers, `ValueTask<T>` for request handlers, `ValueTask<Unit>` + `return Unit.Value;` for requests without a response.
* Generic handlers are not supported by the source generator (only single-parameter open generic notification handlers).
* Pipeline behaviors must be registered explicitly (`IPipelineBehavior<,>`).

### Host (composition root)
Reference `Mediator.SourceGenerator` **only in the host project**; the generator scans the referenced assemblies.
```csharp
services
    .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
    .AddMediatorIntegration(); // IEventPublisher (event sourcing, EventPublishingUowDecorator) + IContractKindClassifier (NBB 4 topics, TenantMiddleware)

services.AddMessagingHost(Configuration, host => host.Configure(config => config
    .AddSubscriberServices(s => s
        .FromMediatorHandledCommands().AddAllClasses()
        .FromMediatorHandledEvents().AddAllClasses())
    .WithDefaultOptions()
    .UsePipeline(p => p
        .UseCorrelationMiddleware()
        .UseExceptionHandlingMiddleware()
        .UseDefaultResiliencyMiddleware()
        .UseMediatorMiddleware())));

services
    .AddProcessManager(assemblies)
    .AddProcessManagerMediatorHandlers(); // NBB.ProcessManager.Mediator
services
    .AddProjectR(assemblies)
    .AddProjectRMediatorHandlers(); // NBB.ProjectR.Mediator
services.AddMediatorEffects();              // NBB.Application.Mediator.Effects (MediatorEff.Send/Publish)
```

Notes:
* **`AddMediatorIntegration()` requires `AddMediator()`** - it registers `MediatorEventPublisher`, which depends on the generated
  `IPublisher`, so the host fails at start (container validation) without it. This also applies to hosts that only publish to the bus
  and use the integration for the contract classifier (NBB 4 topics, `TenantMiddleware`): add the generator and `AddMediator()` there too.
* **Lifetime** - the options passed to `AddMediator` must be compile-time constants. The process manager and ProjectR adapters
  require `ServiceLifetime.Scoped` or `Transient` (their handlers depend on scoped services); the generic host fails at start otherwise.
* **Several generator projects** - the generated `AddMediator()` and `Mediator` types are public by default. When a project with the
  generator is referenced by another one that also has the generator (e.g. a monolith host referencing service hosts), set
  `options.GenerateTypesAsInternal = true` in the referenced project, or keep the generator only in the top-level host.
* **Warning MSG0005** - events handled only by NBB bridges (process managers, projectors) or only published to the bus have no generated
  handler; the generator reports `MSG0005` for them. Suppress it with `<NoWarn>$(NoWarn);MSG0005</NoWarn>` if needed.
* **Dispatch of `object` messages** - `UseMediatorMiddleware()` and `MediatorEventPublisher` use `Publish(object)` / `Send(object)`,
  which work only for message types the source generator saw at compile time (message types from referenced assemblies are included).

### Contract classification (`ContractKind`)
| ContractKind | MediatR | Mediator | NBB 4 topic prefix | TenantMiddleware |
|---|---|---|---|---|
| `Command` | `IRequest` | `IRequest`, `ICommand` | `ch.commands.` | tenant required |
| `Query` | `IRequest<T>` | `IQuery<T>`, `IRequest<T>`, `ICommand<T>` | `ch.messages.` | tenant required |
| `Event` | `INotification` | `INotification` | `ch.events.` | skipped when the tenant is unknown |
| `Other` | anything else | anything else | `ch.messages.` | tenant required |

Equivalent messages get the same NBB 4 topics with both adapters, so a service can move to Mediator independently of the others.

### Mixed fleets
Each application uses a single mediator library (calling both `AddMediatorIntegration()` and `AddMediatRIntegration()` throws). When services on MediatR and services on
Mediator share a contracts assembly, the contracts can implement both marker interfaces during the transition, e.g.
`public record ContractValidated(...) : MediatR.INotification, Mediator.INotification;`.

## NBB 11
The obsolete members will be removed: `MediatorUowDecorator<>` (and the `NBB.Data.Abstractions` reference of `NBB.Application.MediatR`),
`AddMediatorEffects()` and the `Mediator` effects class of `NBB.Application.MediatR.Effects`.
