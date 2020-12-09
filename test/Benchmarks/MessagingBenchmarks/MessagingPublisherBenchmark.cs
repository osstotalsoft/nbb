using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Kafka;
using NBB.Messaging.Nats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingBenchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 10)]
    public class MessagingPublisherBenchmark
    {
        private IServiceProvider _container;

        [GlobalSetup(Target = nameof(KafkaPublish))]
        public void KafkaGlobalSetup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
            }

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddKafkaMessaging();


            services.AddLogging();
            _container = services.BuildServiceProvider();
        }

        [GlobalSetup(Target = nameof(NatsPublish))]
        public void NatsGlobalSetup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
            }

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddNatsMessaging();
            services.AddLogging();
            _container = services.BuildServiceProvider();
        }

        [Benchmark]
        public async Task KafkaPublish()
        {
            using (var scope = _container.CreateScope())
            {
                var pub = scope.ServiceProvider.GetService<IMessageBusPublisher>();
                await pub.PublishAsync(new IntegrationMessage(Guid.NewGuid()),
                    CancellationToken.None);
            }
        }

        [Benchmark]
        public async Task NatsPublish()
        {
            using (var scope = _container.CreateScope())
            {
                var pub = scope.ServiceProvider.GetService<IMessageBusPublisher>();
                await pub.PublishAsync(new IntegrationMessage(Guid.NewGuid()),
                    CancellationToken.None);
            }
        }
    }

    public class IntegrationMessage : IMessage
    {
        public IDictionary<string, string> Headers { get; }

        public object Body { get; }

        public IntegrationMessage(Guid messageId)
        {
            Headers = new Dictionary<string, string>
            {
                [MessagingHeaders.MessageId] = messageId.ToString()
            };
            Body = "";
        }

        public Guid MessageId { get; }
    }
}
