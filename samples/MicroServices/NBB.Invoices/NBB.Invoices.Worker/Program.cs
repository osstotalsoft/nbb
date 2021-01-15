using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Application.MediatR;
using NBB.Core.Abstractions;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Domain.Abstractions;
using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet;
using NBB.Invoices.Application.CommandHandlers;
using NBB.Invoices.Data;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Nats;
using NBB.Resiliency;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Worker
{
    public class Program
    {
        public static async Task Main(string[] _args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables("NETCORE_");
                })
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
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                services.AddMediatR(typeof(InvoiceCommandHandlers).Assembly);
                //services.AddKafkaMessaging();
                services.AddNatsMessaging();
                services.AddInvoicesWriteDataAccess();
                services.AddEventStore()
                    .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                    .WithAdoNetEventRepository();

                services.AddResiliency();

                services.AddMessagingHost()
                    .AddSubscriberServices(config => config
                        .FromMediatRHandledCommands().AddAllClasses()
                        .FromMediatRHandledEvents().AddAllClasses()
                    )
                    .WithDefaultOptions()
                    .UsePipeline(pipelineBuilder => pipelineBuilder
                        .UseCorrelationMiddleware()
                        .UseExceptionHandlingMiddleware()
                        .UseDefaultResiliencyMiddleware()
                        .UseMediatRMiddleware()
                    );

                    //services.AddSingleton<IHostedService, MessageBusSubscriberService<GetInvoice.Query>>();

                    services
                        .Decorate(typeof(IUow<>), typeof(DomainUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(MediatorUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(EventStoreUowDecorator<>));
                });

            await builder.RunConsoleAsync(CancellationToken.None);
        }
    }
}
