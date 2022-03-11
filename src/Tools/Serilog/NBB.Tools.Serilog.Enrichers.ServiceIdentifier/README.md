# NBB.MultiTenancy.Serilog

This project provides a Serilog Enricher that adds the tenant id to the log context

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
    services.AddSingleton<TenantEventLogEnricher>();
    ...
}
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
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },

