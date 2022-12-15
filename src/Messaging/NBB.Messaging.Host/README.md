# Messaging host

The messaging host is a background (hosted) service that receives messages from the configured messaging subscribers and processes them in a pipeline.

## NuGet install
```
dotnet add package NBB.Messaging.Host
```

**Sample usage:**
```csharp
services.AddMessagingHost(
    Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(subscriberBuilder => subscriberBuilder
            .FromMediatRHandledCommands().AddAllClasses()
            .FromMediatRHandledEvents().AddAllClasses()
        )
        .WithDefaultOptions()
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatRMiddleware()
        )
    )
);

```


## Host configuration


### Inline configuration

You can configure the subscribers, options and pipelines using the *Configure* method of the host builder

```csharp
services.AddMessagingHost(
    Configuration, 
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(...)
        .WithOptions(...)
        .UsePipeline(...)
    )
);
```


#### Multiple configurations

In case you need different settings for specific groups of subscribers (eg. different pipelines) you can call *Configure* multiple times:

```csharp

services.AddMessagingHost(
    Configuration,
    hostBuilder =>
    {
        hostBuilder.Configure(configBuilder => configBuilder
            .AddSubscriberServices(...)
            .WithOptions(...)
            .UsePipeline(...)
        );

        hostBuilder.Configure(configBuilder => configBuilder
            .AddSubscriberServices(...)
            .WithOptions(...)
            .UsePipeline(...)
        );
    }
);
```

#### Multiple subscriber groups using the same pipeline

In case you need to use the same pipeline for more subscriber groups, the builder lets you chain multiple `AddSubscriberServices(...).WithOptions(...)` function calls:

```csharp

services.AddMessagingHost(
    Configuration,
    hostBuilder =>
    {
        hostBuilder.Configure(configBuilder => configBuilder
            .AddSubscriberServices(...)
            .WithOptions(...)
            .AddSubscriberServices(...)
            .WithOptions(...)
            .UsePipeline(...)
        );
    }
);
```

#### Register a type dependent pipeline

In case you need to register a type dependent pipeline (like conditional middleware) for more subscribers, you can build something like:

```csharp

services.AddMessagingHost(
    Configuration,
    hostBuilder =>
    {
        hostBuilder.Configure(configBuilder => configBuilder
            .AddSubscriberServices(...)
            .WithOptions(...)
            .UsePipeline((t, p) => p
                .Use(...)
                .When(t == typeof(MyCommand), p => p.Use(...))  //<-- conditional middleware
                .Use(...)
        );
    }
);
``` 

#### Advanced scenarios
There is an overload of the *Configure* method that allows async/await operations.

If you need to resolve services from the DI container you can use the *ApplicationServices* property of the config builder.

```csharp
services.AddMessagingHost(
    Configuration,
    hostBuilder => hostBuilder
    .Configure(async configBuilder =>
    {
        var repository = configBuilder.ApplicationServices.GetRequiredService<IMyRepository>();
        var topics = await repository.GetTopics();
                    
        configBuilder
            .AddSubscriberServices(config => config.FromTopics(topics))
            .WithDefaultOptions()
            .UsePipeline(builder => builder
                .UseCorrelationMiddleware()
                .UseExceptionHandlingMiddleware()
                .UseDefaultResiliencyMiddleware()
                .UseMiddleware<ReceiveEventMediatRMiddleware>());
    }));
```

              

### Using a Startup class

For more complex configuration scenarios you can use a startup class:

```csharp
services.AddMessagingHost(hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());
```

The startup class can use constructor injection and allows using tasks.

```csharp
class MessagingHostStartup : IMessagingHostStartup
{
    private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

    public MessagingHostStartup(IOptions<TenancyHostingOptions> tenancyOptions)
    {
        _tenancyOptions = tenancyOptions;
    }

    public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
    {
        var isMultiTenant = _tenancyOptions?.Value?.TenancyType != TenancyType.None;

        hostConfigurationBuilder
            .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                .FromMediatRHandledCommands().AddAllClasses())
            .WithDefaultOptions()
            .UsePipeline(pipelineBuilder => pipelineBuilder
                .UseCorrelationMiddleware()
                .UseExceptionHandlingMiddleware()
                .When(isMultiTenant, x => x.UseTenantMiddleware())
                .UseDefaultResiliencyMiddleware()
                .UseMediatRMiddleware()
            );

        return Task.CompletedTask;
    }
}
````


## Subscription configuration


### Adding subscribers:
`AddSubscriberServices(Action<ITypeSourceSelector> builder)` registers messagebus subscriber services for messages from the following sources:
The fluent API for configuration starts with specifying the sources of message types/topics
- Assemblies: scans the specified assemblies for message types.
   - `FromAssemblyOf<T>()`
   - `FromCallingAssembly()`
   - `FromExecutingAssembly()`
   - `FromEntryAssembly()`
   - `FromAssembliesOf(params Type[] types)`
   - `FromAssembliesOf(IEnumerable<Type> types)`
   - `FromAssemblies(params Assembly[] assemblies)`
   - `FromAssemblies(IEnumerable<Assembly> assemblies)`
- Topics: specifies the messaging topic(s) 
   - `FromTopic(string topic)`
   - `FromTopics(params string[] topics)`
   - `FromTopics(IEnumerable<string> topics)`
- Individual types:
   - `AddType<TMessage>()`
   - `AddTypes(params Type[] types)`
   - `AddTypes(IEnumerable<Type> types)`
- MediatR handled messages: finds MediatR handlers registered in the IoC container and extracts the handled types:
   - `FromMediatRHandledEvents()`
   - `FromMediatRHandledCommands()`
   - `FromMediatRHandledQueries()`
   - `FromMediatRHandledMessages()` - includes all handled types (commands, events, queries)

For the Assembly and MediatR sources, the types should be selected using the following methods:
   - `AddAllClasses(bool publicOnly = true)` - selects all (public) types from the current source 
   - `AddClassesAssignableTo<TBase>(bool publicOnly = true)` - selects all (public) types from the current source that inherit/implement TBase
   - `AddClassesWhere(Func<Type, bool> predicate, bool publicOnly = true)` - selects all (public) types that match the predicate

**Examples**

Add subscribers for all messages that are handled by a registered MediatR request or notification handler:
* *notice that registrations can be chained*
```csharp
.AddSubscriberServices(subscriberBuilder => subscriberBuilder
    .FromMediatRHandledCommands().AddAllClasses()
    .FromMediatRHandledEvents().AddAllClasses())
```

Add subscribers for the specified topics:

```csharp
.AddSubscriberServices(subscriberBuilder => subscriberBuilder
    .FromTopics("Topic1", "Topic2"))
```

Add a subscriber for messages of a speciffic type:
```csharp
.AddSubscriberServices(subscriberBuilder => subscriberBuilder
    .AddType<MyMessage>())
```

Add subscribers for all classes in an assembly that implement a specific interface:
```csharp
.AddSubscriberServices(subscriberBuilder => subscriberBuilder
    .FromAssemblyOf<MyMessage>().AddClassesAssignableTo<IMyInterface>())
```

Add subscribers for classes in the entry assembly that have a specific namespace

```csharp
.AddSubscriberServices(subscriberBuilder => subscriberBuilder
    .FromEntryAssembly().AddClassesWhere(t => t.Namespace == "MyApp.Messages"))
```

###  Options for subscriber/consumer configuration:
When adding subscriber services the subscription options should be specified

#### Custom options
Configure transport options (*SubscriptionTransportOptions*):

```csharp
.WithOptions(optionsBuilder => optionsBuilder
    .ConfigureTransport(transportOptions =>
        transportOptions with {MaxConcurrentMessages = 2}))
```

Add dynamic deserialization 

```csharp
.WithOptions(optionsBuilder => optionsBuilder
    .UseDynamicDeserialization(new[] { typeof(MyMessage).Assembly }))
```

For more details see [`Subscriber options`](../NBB.Messaging.Abstractions#subscriber-options)

#### Default options
Use default transport and serialization options


```csharp
.WithDefaultOptions()
```

For more details see [`Subscriber options`](../NBB.Messaging.Abstractions#subscriber-options)

## Pipeline configuration

The pipeline builder allows plugging in various custom or built-in middleware.

### Custom middleware

#### Class middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseMiddleware<MyCustomMiddleware>())
```

The custom middleware class should implement the interface `IPipelineMiddleware<MessagingContext>` and can have constructor injected parameters.


#### Inline function middleware

Adds a middleware delegate defined in-line to the message pipeline.
```csharp
.UsePipeline(pipelineBuilder => 
    pipelineBuilder.Use(async (messagingContext, cancellationToken, next) => {
        ...
        await next();
    })
)

```
   

### Built-in middleware

The messaging host provides some built in middleware

#### built-in correlation middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseCorrelationMiddleware())
```


Typically configured early in the pipeline, it has the role to fetch the correlation id from the received message or create a new one if the incoming message does not have one.
#### built-in exception handling middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseExceptionHandlingMiddleware())
```

Typically configured very early in the pipeline, it swallows exceptions and logs them to the console
#### built-in resiliency middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseDefaultResiliencyMiddleware())
```

Includes the following resiliency policies for incoming messages:
* Retry forever when a **ConcurrencyException** is received
* Retry three times with a progressive delay when an **OutOfOrderException** is received
#### built-in MediatR middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseMediatRMiddleware())
```
Tipically configured last in the pipeline, it acts as a message dispatcher (broker) that delivers messages to MediatR handlers

#### built-in Multi Tenant middleware

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.UseTenantMiddleware())
```

It identifies the tenant from the incoming message, preforms various validations against the current multi-tenant configuration, and creates the tenancy context.

To use it you must reference the *NBB.Messaging.MultiTenancy* package


### Conditional middleware registration

```csharp
.UsePipeline(pipelineBuilder => pipelineBuilder.When(isMultiTenant, x => x.UseTenantMiddleware()))
```

Registers a middleware only when the condition is met. This allows configuring the pipeline based on a deployment specific configuration.

It can be used to register any custom or built-in middleware previously described.


## Messaging context

When processing incoming messages we have access to a messaging context that contains:
- the received message envelope (including payload and headers)
- the topic of the incoming message

In the pipeline middleware we have direct access to the messaging context as a parameter

To access the context from other contexts - like a MediatR handler we must inject the *MessagingContextAccessor*

```csharp
public class MyHandler : IRequestHandler<MyCommand>
{
    private readonly MessagingContextAccessor _messagingContextAccessor;

    public MyHandler(MessagingContextAccessor messagingContextAccessor)
    {
        _messagingContextAccessor = messagingContextAccessor;
    }

    public Task<Unit> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var headers = _messagingContextAccessor.MessagingContext.MessagingEnvelope.Headers;
        ...
    }
}
```

## Transport error handler
The messaging host provides two builtin transport error strategies:
 - Retry: tries to restart the messaging host for 10 times before throwing an error and shutting down the host. You can set the number of retries in 'appsettings.json' by setting the `Messaging.Host.StartRetryCount` environment variable
 - Throw: throws an error and shuts down the host

You can set one or the other in 'appsettings.json' by setting`Messaging.Host.TransportErrorStrategy`. By default it uses the `TransportErrorStrategy.Retry` handler.

Sample host configuration in `appsettings.json

```json
{  
    "Messaging": {
        ...   
        "Host": {
            "TransportErrorStrategy": "Retry",
            "StartRetryCount": 10
        }
    }
}
```

