# NBB.Correlation.Serilog.SqlServer

This package enables persisting the logs along with the current correlation ID to a SQL server table.
The correlation ID is persisted separate column which facilitates filtering the table.

## NuGet install
```
dotnet add package NBB.Serilog.SqlServer
```

To configure the databse persistance use the following extension in  the serilog configuration:

```csharp
Log.Logger = new LoggerConfiguration()
   ...
   .WriteTo.MsSqlServerWithCorrelation(connectionString, "__Logs", autoCreateSqlTable: true, columnOptions: columnOptions)
```

The configuration options are detailed in the package [`Serilog.Sinks.MSSqlServer`](https://github.com/serilog/serilog-sinks-mssqlserver#readme)
