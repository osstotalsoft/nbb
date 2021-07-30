using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;
using Serilog.Sinks.MSSqlServer;
using NBB.Correlation.Serilog;
using NBB.Messaging.Host;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.MicroServicesOrchestration
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
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
                    services.AddMediatR(typeof(Program).Assembly);

                    services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);

                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer()
                        .WithAdoNetEventRepository();

                    var integrationMessageAssemblies = new[] {
                        typeof(NBB.Contracts.PublishedLanguage.ContractValidated).Assembly,
                        typeof(NBB.Invoices.PublishedLanguage.InvoiceCreated).Assembly,
                        typeof(NBB.Payments.PublishedLanguage.PayableCreated).Assembly,
                    };

                    services.AddMessagingHost(hostBuilder => hostBuilder
                        .Configure(configBuilder => configBuilder
                            .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                                .FromMediatRHandledEvents().AddClassesWhere(t => integrationMessageAssemblies.Contains(t.Assembly))
                            )
                            .WithDefaultOptions()
                            .UsePipeline(pipelineBuilder => pipelineBuilder
                                .UseCorrelationMiddleware()
                                .UseExceptionHandlingMiddleware()
                                .UseDefaultResiliencyMiddleware()
                                .UseMediatRMiddleware()
                            )
                        ));

                    services.AddProcessManager(Assembly.GetEntryAssembly());
                });

            var host = builder.UseConsoleLifetime().Build();

            await host.RunAsync();
        }
    }
}