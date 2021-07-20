using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Migrations;

namespace TheBenchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 10)]
    public class EventRepositoryBenchmark
    {
        private IServiceProvider _container;

        [GlobalSetup(Target = nameof(AdoNetEventRepositorySave))]
        public void GlobalSetupAdoNetEventRepositorySave()
        {
            MigrateDb();

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
            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithAdoNetEventRepository();
            _container = services.BuildServiceProvider();
        }

        //[Benchmark]
        public void AdoNetEventRepositorySave()
        {
            using (var scope = _container.CreateScope())
            {
                var repository = scope.ServiceProvider.GetService<IEventRepository>();
                repository.AppendEventsToStreamAsync("stream", new[] { new EventDescriptor(Guid.NewGuid(), "some event type", "jsdjf wefkjwe hfjwehfj", "stream", null) }, null, CancellationToken.None).Wait();
            }

        }

        [GlobalSetup(Target = nameof(AdoNetEventRepositoryLoad))]
        public void GlobalSetupAdoNetEventRepositoryLoad()
        {
            MigrateDb();

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
            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithAdoNetEventRepository();
            _container = services.BuildServiceProvider();

            using (var scope = _container.CreateScope())
            {
                var repository = scope.ServiceProvider.GetService<IEventRepository>();
                repository.AppendEventsToStreamAsync("stream", GetMockData(), null, CancellationToken.None).Wait();
            }

            Console.WriteLine("inserted");

        }

        [Benchmark]
        public void AdoNetEventRepositoryLoad()
        {
            using (var scope = _container.CreateScope())
            {
                var repository = scope.ServiceProvider.GetService<IEventRepository>();
                repository.GetEventsFromStreamAsync("73", null, CancellationToken.None).Wait();
            }

        }




        private static void MigrateDb()
        {
            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(default).Wait();
        }

        private static List<EventDescriptor> GetMockData()
        {
            var eventDescriptors = new List<EventDescriptor>();
            for (var stream = 0; stream < 1000; stream++)
            {
                for (var sequenceNo = 0; sequenceNo < 100; sequenceNo++)
                {
                    eventDescriptors.Add(new EventDescriptor(Guid.NewGuid(), "some event type", "jsdjf wefkjwe hfjwehfj", "stream", null));
                }
            }

            return eventDescriptors;
        }
    }
}
