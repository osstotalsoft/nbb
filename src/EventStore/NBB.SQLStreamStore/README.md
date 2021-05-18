# NBB.SQLStreamStore

This package provides a [`SQL Strem Store`](https://github.com/SQLStreamStore/SQLStreamStore#readme) implementation of the *Event Store* abstractions as defined in package [`NBB.EventStore.Abstractions`](../NBB.EventStore.Abstractions).

## NuGet install
```
dotnet add package NBB.SQLStreamStore
```

## Usage

The event store must be registered in the DI container:

```csharp
services.AddSqlStreamStore();
```