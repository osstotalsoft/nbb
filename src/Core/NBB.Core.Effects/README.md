# NBB.Core.Effects

Pure functional, type-safe, composable asynchronous and concurrent programming for .net

## From side-effect to effect
An Effect system is an api that allows you to represent side-effects at type level, without actually performing them, so that you can model and compose side-effectfull processes using just pure functions.

## NuGet install
```
dotnet add package NBB.Core.Effects
```

## So how do you create effects?

### Effect.Pure
```csharp
var eff = Effect.Pure(5)
```

### Effects.from impure functions
```csharp
var eff = Effect.From(System.Guid.NewGuid);
```

### Effect.Of<SideEffect>
```csharp
static class ConsoleEffects
{
    internal class WriteLine
    {
        internal record SideEffect(string Value) : ISideEffect;

        internal class Handler : ISideEffectHandler<SideEffect, Unit>
        {
            public Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = new CancellationToken())
            {
                System.Console.WriteLine(sideEffect.Value);
                return Unit.Task;
            }
        }
    }

    internal class ReadLine
    {
        internal record SideEffect : ISideEffect<string>;

        internal static string Handle(SideEffect _) => System.Console.ReadLine();
    }
}

public static class Console
{
    public static Effect<Unit> WriteLine(string value) =>
        Effect.Of<ConsoleEffects.WriteLine.SideEffect, Unit>(new(value));

    public static Effect<string> ReadLine = Effect.Of<ConsoleEffects.ReadLine.SideEffect, string>(new());
}
```

## Registering custom effect handlers
```csharp
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConsoleEffects(this IServiceCollection services)
    {
        services
            .AddSingleton<ISideEffectHandler<ConsoleEffects.WriteLine.SideEffect, Unit>,
                ConsoleEffects.WriteLine.Handler>()
            .AddSideEffectHandler<ConsoleEffects.ReadLine.SideEffect, string>(ConsoleEffects.ReadLine.Handle);
        return services;
    }
}
```


## Composing effects
Effect compisition is based on the functor, applicative and monad algebras

### Chaining effects with Then (Map, Bind)
You can map/bind the value inside an effect using Then.
```csharp
public static readonly Effect<Unit> Main =
    Console.WriteLine("What is your name?")
        .Then(Console.ReadLine)
        .Then(name => Guid.NewGuid.Then(userId => Greet(name, userId)));
```

### Sequencing effects
Sequence produces an effect from a sequence of effects, when interpreted, it will interpret the list of effects one after the other.
```csharp
public static readonly Effect<Unit> Main =
    Effect.Sequence(new List<Effect<Unit>>
    {
        Console.WriteLine("Hello!"),
        Console.WriteLine("Good bye!"),
    });
```

### Concurent effects
Produces a compound effect, that when interpreted, it will concurently interpret the two composed effects
```csharp
public static readonly Effect<Unit> Main =
    Effect.Parallel(
        Console.WriteLine("Hello!"),
        Console.WriteLine("Hello2!"));
```

### LINQ to effects
```csharp
public static readonly Effect<Unit> Main =
    from id in Guid.NewGuid
    from name in GetNameById(id)
    let name1 = name.ToUpper()
    from _ in Console.WriteLine(name1)
    select Unit.Value;
```

## Interpreting effects
Usually effect interpretation is done once in the programm entrypoint.
```csharp
using NBB.Core.Effects;

await using var interpreter = Interpreter.CreateDefault(services => services.AddConsoleEffects());
await interpreter.Interpret(Program.Main);
```




