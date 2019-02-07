The worker process
===============

The worker process is used to offload time-consuming workloads from Web, API or CLI processes and handle messaging subscriptions.
Although it is not mandatory we strongly encourage you to have at least one worker process.

In order to build a worker you need to reference the package *NBB.Worker* that exposes services that auto-subcribe to the according topics and transport messages to the appropriate handlers.
Note that the worker is just an adapter from the Clean perspective, in the way that it adapts messaging messages to application use cases.
An example worker:
```csharp
public static async Task MainAsync(string[] args)
{
    var builder = new HostBuilder()
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddEnvironmentVariables();
        })
        .ConfigureLogging((hostingContext, loggingBuilder) =>
        {
            var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationLogEventEnricher>()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(connectionString, "Logs", autoCreateSqlTable: true)
                .CreateLogger();

            loggingBuilder.AddSerilog(dispose: true);
        })
        .ConfigureServices((hostingContext, services) =>
        {
            services.AddContractsApplication();
            services.AddWorkerServices();
        });

    await builder.RunConsoleAsync();
}
```