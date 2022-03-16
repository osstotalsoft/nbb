# NBB.Tools.Serilog.Enrichers.TenantId

This project provides a Serilog Enricher that adds the tenant id to the log context.
It can be the case that logging is requested before tenant identification is requested or that tenant cannot be found. In this case, the empty guid will be set in the context.


## NuGet install
```
dotnet add package NBB.MultiTenancy.Serilog
```
# Registration
The enricher should be registered in `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddSingleton<TenantEnricher>();
    ...
}
```
# Usage: example for Program.cs
```csharp
var hostBuilder = CreateHostBuilder(args);
var tempLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(Configuration)
    .CreateLogger();

hostBuilder.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(Configuration);
    configuration.Enrich.With(services.GetRequiredService<TenantEnricher>());
    ...
});
```

# Usage: example for an sql logger configured in appsettings.json
  "Serilog": {
    "MinimumLevel": "Debug",
    "tableName": "TABLE_NAME",
    "WriteTo": [
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Server=SERVER,POST;Database=DB;User Id=USERID;Password=PASS;MultipleActiveResultSets=true",
          "tableName": "TABLE_NAME",
          "autoCreateSqlTable": false,
          "columnOptionsSection": {
            "addStandardColumns": [ "LogEvent" ],
            "customColumns": [
              {
                "ColumnName": "TenantId",
                "PropertyName": "TenantId",
                "DataType": "uniqueidentifier",
                "AllowNull": false
              },
              {
                "ColumnName": "OtherId",
                "PropertyName": "OtherId",
                "DataType": "uniqueidentifier",
                "AllowNull": true
              }
            ]
          }
        }
      },
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message} T: {TenantId} E: {EnvelopeGuid} N: {FirstName} {LastName} ID: {IdCardIdentifier} EID: {FingerprintId} {NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "Tenant" ]
  },

