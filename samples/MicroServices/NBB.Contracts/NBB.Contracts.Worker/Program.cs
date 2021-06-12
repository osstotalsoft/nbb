using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Nats;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Threading.Tasks;
using NBB.Messaging.Host.Builder;
using Serilog.Sinks.MSSqlServer;
using Microsoft.Extensions.Options;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static async Task Main(string[] _args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables("NETCORE_"); })
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json", true);
                    configurationBuilder.AddEnvironmentVariables();

                    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
                    {
                        configurationBuilder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString,
                            new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(ContractCommandHandlers).Assembly);

                    services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] { new SingleValueObjectConverter() })
                        .WithAdoNetEventRepository();

                    services.AddMessagingHost(hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                });

            var host = builder.UseConsoleLifetime().Build();

            await host.RunAsync();
        }
    }

    class MessagingHostStartup : IMessagingHostStartup
    {
        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
        {
            hostConfigurationBuilder
                .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                    .FromMediatRHandledCommands().AddAllClasses())
                .WithOptions(optionsBuilder => optionsBuilder
                    .ConfigureTransport(transportOptions =>
                        transportOptions with { MaxConcurrentMessages = 2 }))
                .UsePipeline(pipelineBuilder => pipelineBuilder
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware()
                    .UseDefaultResiliencyMiddleware()
                    .UseMediatRMiddleware()
                );

            return Task.CompletedTask;
        }
    }
}