using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Domain.Abstractions;
using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Kafka;
using NBB.Payments.Application.CommandHandlers;
using NBB.Payments.Data;
using NBB.Resiliency;
using Serilog;
using Serilog.Events;

namespace NBB.Payments.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
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
                        .WriteTo.MSSqlServer(connectionString, "Logs", autoCreateSqlTable: true)
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(PayableCommandHandlers).Assembly);
                    services.AddKafkaMessaging();

                    services.AddPaymentsWriteDataAccess();

                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                        .WithAdoNetEventRepository();

                    services.AddResiliency();

                    services.AddMessagingHost()
                        .AddSubscriberServices(config => config
                            .FromMediatRHandledCommands().AddClassesAssignableTo<Command>()
                            .FromMediatRHandledEvents().AddClassesAssignableTo<Event>()
                        )
                        .WithDefaultOptions()
                        .UsePipeline(pipelineBuilder => pipelineBuilder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware()
                        );

                    services
                        .Decorate(typeof(IUow<>), typeof(DomainUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(MediatorUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(EventStoreUowDecorator<>));
                });

            await builder.RunConsoleAsync();
        }
    }
}
