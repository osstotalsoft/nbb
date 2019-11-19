using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.Domain.ContractAggregate;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation;
using NBB.Correlation.Serilog;
using NBB.Data.EventSourcing;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Internal;
using NBB.EventStore.Internal;
using NBB.EventStore.MessagingExtensions;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Mediator.OpenFaaS;
using NBB.Mediator.OpenFaaS.Extensions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats;
using NBB.Messaging.Nats.Internal;
using Serilog;
using Serilog.Events;

namespace NBB.Contracts.Functions.CreateContract
{
    public class Function
    {
        private ServiceProvider _container;
     
        public void PrepareFunctionContext()
        {
            _container = BuildServiceProvider();
        }

        public async Task Invoke(string body, CancellationToken cancellationToken)
        {
            var correlationId = GetCorrelationIdFromBuffer(body);
            using (CorrelationManager.NewCorrelationId(correlationId))
            {
                using (var scope = _container.CreateScope())
                {
                    var command = GetCommandFromBuffer(body);
                    var commandHandler = scope.ServiceProvider.GetService<IRequestHandler<Application.Commands.CreateContract>>();
                    if (commandHandler != null)
                    {
                        await commandHandler.Handle(command, cancellationToken);
                    }
                }
            }

            //using (CorrelationManager.NewCorrelationId(correlationId))
            //{
            //    var command = GetCommandFromBuffer(buffer);
            //    var commandHandler = GetCommandHandler();
            //    await commandHandler.Handle(command, CancellationToken.None);
            //    await commandHandler.Handle(command, CancellationToken.None);
            //}

        }

        private static ServiceProvider BuildServiceProvider()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();


            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.With<CorrelationLogEventEnricher>()
                    //.WriteTo.Console()
                    .WriteTo.MSSqlServer(configuration.GetConnectionString("Logs"), "Logs", autoCreateSqlTable: true)
                    .CreateLogger();

                builder.AddSerilog(dispose: true);
            });

            services.AddOpenFaaSMediator();
            //services.AddKafkaMessaging();
            services.AddNatsMessaging();
            services.AddContractsWriteModelDataAccess();

            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                .WithAdoNetEventRepository()
                .WithMessagingExtensions();

            services.AddScoped<IRequestHandler<NBB.Contracts.Application.Commands.CreateContract>, ContractCommandHandlers>();

            var container = services.BuildServiceProvider();
            return container;
        }


        private static ContractCommandHandlers GetCommandHandler()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationLogEventEnricher>()
                .WriteTo.MSSqlServer(configuration.GetConnectionString("Logs"), "Logs", autoCreateSqlTable: true)
                .CreateLogger();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();

            var stanConnectionProvider = new StanConnectionProvider(configuration, new Logger<StanConnectionProvider>(loggerFactory), null);
            var topicRegistry = new DefaultTopicRegistry(configuration);
            var messageTypeRegistry = new DefaultMessageTypeRegistry();
            var messageSerdes = new NewtonsoftJsonMessageSerDes(messageTypeRegistry);
            var messagingTopicPublisher = new NatsMessagingTopicPublisher(stanConnectionProvider, new Logger<NatsMessagingTopicPublisher>(loggerFactory));
            var messageBusPublisher = new MessageBusPublisher(messagingTopicPublisher, topicRegistry, messageSerdes, configuration, new Logger<MessageBusPublisher>(loggerFactory));

            var scripts = new Scripts();
            var eventRepository = new AdoNetEventRepository(scripts, configuration, new Logger<AdoNetEventRepository>(loggerFactory));

            var eventStoreSerDes = new NewtonsoftJsonEventStoreSerDes();
            var eventStore = new EventStore.EventStore(eventRepository, eventStoreSerDes, new Logger<EventStore.EventStore>(loggerFactory));
            var eventStoreDecorator = new MessagingEventStoreDecorator(eventStore, messageBusPublisher, new MessagingTopicResolver(configuration));
            var mediator = new OpenFaaSMediator(configuration, new Logger<OpenFaaSMediator>(loggerFactory));
            var repository = new EventSourcedRepository<Contract>(eventStoreDecorator, null, mediator, new EventSourcingOptions(),  new Logger<EventSourcedRepository<Contract>>(loggerFactory));
            var result = new ContractCommandHandlers(repository);

            //disposables = new List<IDisposable>{stanConnectionProvider};

            return result;
        }


        private static Guid? GetCorrelationIdFromBuffer(string buffer)
        {
            return null;
        }

        private static NBB.Contracts.Application.Commands.CreateContract GetCommandFromBuffer(string buffer)
        {
            return new NBB.Contracts.Application.Commands.CreateContract(Guid.NewGuid());
        }

    }
}
