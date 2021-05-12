# NBB.Core.Pipeline

A multi-purpose pipeline builder

## NuGet install
```
dotnet add package NBB.Core.Pipeline
```

## Philosophy
A pipeline is a function of some context, composed from one or multiple middlewares.
Each middleware receives the context and the next middleware, and is responsible of how and when to invoke the next middleware.

## Design pattern
**Chain of Responsibility** is a behavioral design pattern that lets you pass requests along a chain of handlers. Upon receiving a request, each handler decides either to process the request or to pass it to the next handler in the chain.

## Pipeline middleware
Create the middleware class by implementing the *IPipelineMiddleware* interface:
```csharp
private class SomeMiddleware : IPipelineMiddleware<IContext>
{
    public async Task Invoke(IContext ctx, CancellationToken cancellationToken, Func<Task> next)
    {
        ctx.Log.Add("FirstBefore");
        await next();
        ctx.Log.Add("FirstAfter");
    }
}
```
Now register the middleware with the pipeline:
```csharp
//somewhere in a func
var pipeline = new PipelineBuilder<IContext>()
    .UseMiddleware<SomeMiddleware, IContext>()
    .UseMiddleware<SomeOtherMiddleware, IContext>()
    .Pipeline;
```

## Functional pipeline middleware
If you don't require constructor injection you can model the middleware using just a function.
```csharp
var pipeline = new PipelineBuilder<IContext>()
    .Use(async (ctx, cancellationToken, next) =>
    {
        ctx.Log.Add("FirstBefore");
        await next();
        Thread.Sleep(100);
        ctx.Log.Add("FirstAfter");
    })
    .Use(async (data, cancellationToken, next) =>
    {
        ctx.Log.Add("SecondBefore");
        await next();
        ctx.Log.Add("SecondAfter");
    })
    .Pipeline;
```

## Pipeline context
You can use whatever context type for your pipeline, the only restriction is to expose an instance of the IServiceProvider
```csharp
public interface IPipelineContext
{
    IServiceProvider Services { get; }
}

public record MessagingContext
    (MessagingEnvelope MessagingEnvelope, string TopicName, IServiceProvider Services) : IPipelineContext;
```

## Pipeline execution
The PipelineBuilder.Pipeline returns just a function, that when executed executes the pipeline
```csharp
var pipeline = new PipelineBuilder<SomeContext>()
    .UseMiddleware<FirstMiddleware, SomeContext>()
    .Pipeline;

await pipeline(new SomeContext(), default);
```

