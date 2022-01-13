// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentValidation;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using NBB.Messaging.OpenTracing.Publisher;
using NBB.Messaging.OpenTracing.Subscriber;
using OpenTracing;
using OpenTracing.Noop;
using OpenTracing.Util;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var env = hostingContext.HostingEnvironment.IsDevelopment();
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
                    services.Scan(scan => scan
                        .FromAssemblyOf<ContractCommandHandlers>()
                        .AddClasses(classes => classes.AssignableTo<IValidator>())
                        .AsImplementedInterfaces()
                        .WithScopedLifetime());

                    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));

                    var transport = hostingContext.Configuration.GetValue("Messaging:Transport", "NATS");
                    if (transport.Equals("NATS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services
                            .AddMessageBus()
                            .AddNatsTransport(hostingContext.Configuration)
                            .UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services
                            .AddMessageBus()
                            .AddRusiTransport(hostingContext.Configuration)
                            .UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else
                    {
                        throw new Exception($"Messaging:Transport={transport} not supported");
                    }

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] { new SingleValueObjectConverter() })
                        .WithAdoNetEventRepository();

                    services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();
                    services.AddMessagingHost(hostingContext.Configuration, hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                    // OpenTracing
                    services.AddOpenTracingCoreServices(builder => builder.AddGenericDiagnostics().AddMicrosoftSqlClient());


                    services.AddSingleton<ITracer>(serviceProvider =>
                    {
                        if (!hostingContext.Configuration.GetValue<bool>("OpenTracing:Jaeger:IsEnabled"))
                        {
                            return NoopTracerFactory.Create();
                        }

                        string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                        ITracer tracer = new Tracer.Builder(serviceName)
                            .WithLoggerFactory(loggerFactory)
                            .WithSampler(new ConstSampler(true))
                            .WithReporter(new RemoteReporter.Builder()
                                .WithSender(new HttpSender(hostingContext.Configuration.GetValue<string>("OpenTracing:Jaeger:CollectorUrl")))
                                .Build())
                            .Build();

                        GlobalTracer.Register(tracer);

                        return tracer;
                    });
        
                });

            var host = builder.Build();

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
                    .UseMiddleware<OpenTracingMiddleware>()
                    .UseDefaultResiliencyMiddleware()
                    .UseMediatRMiddleware()
                );

            return Task.CompletedTask;
        }
    }
}
