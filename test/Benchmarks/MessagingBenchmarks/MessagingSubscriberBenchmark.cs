using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka;
using NBB.Messaging.Nats;

namespace MessagingBenchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 1)]
    public class MessagingSubscriberBenchmark
    {
        private IServiceProvider _container;
        private readonly int _msgsCnt = 1000;

        [GlobalSetup(Target = nameof(KafkaSubcribeTest))]
        public void KafkaGlobalSetup()
        {
            var services = GetServices();
            services
                .AddKafkaMessaging();

            _container = services.BuildServiceProvider();
            SeedTopic();
        }

        [GlobalSetup(Target = nameof(NatsSubscribeTest))]
        public void NatsGlobalSetup()
        {
            var services = GetServices();
            services.AddNatsMessaging();
            _container = services.BuildServiceProvider();
            SeedTopic();
        }

        [Benchmark]
        public Task KafkaSubcribeTest()
        {
            return SubscribeAndReceiveMsgs();
        }

        //[Benchmark]
        public Task NatsSubscribeTest()
        {
            return SubscribeAndReceiveMsgs();
        }



        private static IServiceCollection GetServices()
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
            services.AddLogging();

            return services;
        }

        private void SeedTopic()
        {
            using (var scope = _container.CreateScope())
            {
                var pub = scope.ServiceProvider.GetService<IMessageBusPublisher>();
                var messages = Enumerable.Range(0, _msgsCnt)
                    .Select(i => new IntegrationMessage(Guid.NewGuid()));

                Parallel.ForEach(messages, async m =>
                {
                    await pub.PublishAsync(m, CancellationToken.None);

                });
            }
        }

        private async Task SubscribeAndReceiveMsgs()
        {
            using (var scope = _container.CreateScope())
            {
                var cnt = 0;
                var cts = new CancellationTokenSource();
                var sub = scope.ServiceProvider.GetService<IMessageBusSubscriber<IntegrationMessage>>();

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                await sub.SubscribeAsync(async e =>
                {
                    stopWatch.Stop();
                    var ms = stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"Message {cnt} received in {ms} miliseconds");
                    await Task.Delay(TimeSpan.FromMilliseconds(10), cts.Token);
                    if(cnt == _msgsCnt)
                        cts.Cancel();
                    cnt++;
                    stopWatch.Restart();
                }, cts.Token);
            }
        }


    }
}
