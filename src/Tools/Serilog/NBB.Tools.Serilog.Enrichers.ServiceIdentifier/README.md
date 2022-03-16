# NBB.Tools.Serilog.Enrichers.ServiceIdentifier

This project provides a Serilog Enricher that adds a service identifier to the log context.
The identifier is taken either from messaging source or from the Assembly.GetEntryAssembly().Name.


## NuGet install
```
dotnet add package NBB.Tools.Serilog.Enrichers.ServiceIdentifier
```
# Registration
The enricher should be registered in `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddSingleton<ServiceIdentifierEnricher>();
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
    configuration.Enrich.With(services.GetRequiredService<ServiceIdentifierEnricher>());
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
                "ColumnName": "ServiceIdentifier",
                "PropertyName": "ServiceIdentifier",
                "DataType": "nvarchar",
                "DataLength": "255",
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
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "ServiceIdentifier" ]
  },

