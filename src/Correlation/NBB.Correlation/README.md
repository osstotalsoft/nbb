# NBB.Correlation

This package provides core correlation functionality.

## NuGet install
```
dotnet add package NBB.Correlation
```

## Correlation manager

The correlation manager handles the current correlation ID. This correlation ID is wrapped inside a correlation scope. 

### Create a correlation scope
Creates a correlation scope from the given correlation ID. If a correlation ID is not provided, a new one is generated. 

When the scope is disposed the current correlation ID is cleared.

Usually the scope creation and disposing should be handled in infrastructure code.

```csharp
using (CorrelationManager.NewCorrelationId(correlationId))
{
    ...
}
```

### Get the current correlation ID
Gets the current correlation ID if a correlation scope is opened. Otherwise it returns *null*.


```csharp
Guid? correlationId = CorrelationManager.GetCorrelationId()
```


