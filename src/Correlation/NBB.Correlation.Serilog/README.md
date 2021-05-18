# NBB.Correlation.Serilog

This package is used to add the current correlation ID as a property in the Serilog structured logs.

## NuGet install
```
dotnet add package NBB.Correlation.Serilog
```

## Log event enricher

The provided log event enricher should be added to the serolog configuration.

```csharp
Log.Logger = new LoggerConfiguration()
    ...
    .Enrich.With<CorrelationLogEventEnricher>()
```