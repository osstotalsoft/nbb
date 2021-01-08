﻿using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Migrations;
using NBB.EventStore.AdoNet.Multitenancy;
using NBB.GetEventStore;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.SQLStreamStore;
using NBB.SQLStreamStore.Migrations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace TheBenchmarks
{
    public class TestEvent : IEvent
    {
        public Guid EventId { get; set; }

        public Guid? CorrelationId { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public void SetCorrelationId(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }


    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 10)]
    public class EventStoreBenchmark
    {
        private IServiceProvider _container;
        private readonly string _loadTestStream = Guid.NewGuid().ToString();


        [GlobalSetup(Target = nameof(NBBEventStoreSave))]
        public void GlobalSetupNBBEventStoreSave()
        {
            MigrateNBBEventStore(false);

            _container = BuildServiceProvider((services, _) =>
                services.AddEventStore()
                    .WithNewtownsoftJsonEventStoreSeserializer()
                    .WithAdoNetEventRepository());
        }

        [GlobalSetup(Target = nameof(NBBMultiTenantEventStoreSave))]
        public void GlobalSetupNBBMultiTenantEventStoreSave()
        {
            MigrateNBBEventStore(true);

            _container = BuildServiceProvider((services, configuration) =>
                services.AddEventStore()
                    .WithNewtownsoftJsonEventStoreSeserializer()
                    .AddSingleton<ITenantContextAccessor, TenantContextAccessorMock>()
                    .WithMultiTenantAdoNetEventRepository());
         
        }

        [GlobalSetup(Target = nameof(SqlStreamStoreSave))]
        public void GlobalSetupSqlStreamStoreSave()
        {
            MigrateSqlStreamStore();

            _container = BuildServiceProvider((services, _) =>
                services.AddSqlStreamStore());
        }

        [GlobalSetup(Target = nameof(GetEventStoreSave))]
        public void GlobalSetupGetEventStoreSave()
        {
            _container = BuildServiceProvider((services, _) =>
                services.AddGetEventStore());
        }


        [GlobalSetup(Target = nameof(NBBEventStoreLoad))]
        public void GlobalSetupNBBEventStoreLoad()
        {
            GlobalSetupNBBEventStoreSave();
            SeedEventRepository(_loadTestStream);
        }

        [GlobalSetup(Target = nameof(NBBMultiTenantEventStoreLoad))]
        public void GlobalMultiTenantSetupNBBEventStoreLoad()
        {
            GlobalSetupNBBMultiTenantEventStoreSave();
            SeedEventRepository(_loadTestStream);
        }

        [GlobalSetup(Target = nameof(SqlStreamStoreLoad))]
        public void GlobalSetupSqlStreamStoreLoad()
        {
            GlobalSetupSqlStreamStoreSave();
            SeedEventRepository(_loadTestStream);
        }

        [GlobalSetup(Target = nameof(GetEventStoreLoad))]
        public void GlobalSetupGetEventStoreLoad()
        {
            GlobalSetupGetEventStoreSave();
            SeedEventRepository(_loadTestStream);
        }

        [Benchmark]
        public void NBBEventStoreSave()
        {
            EventStoreSave();
        }

        [Benchmark]
        public void NBBMultiTenantEventStoreSave()
        {
            EventStoreSave();
        }

        [Benchmark]
        public void SqlStreamStoreSave()
        {
            EventStoreSave();
        }

        //[Benchmark]
        public void GetEventStoreSave()
        {
            EventStoreSave();
        }

        [Benchmark]
        public void NBBEventStoreLoad()
        {
            EventStoreLoad();
        }

        [Benchmark]
        public void NBBMultiTenantEventStoreLoad()
        {
            EventStoreLoad();
        }

        [Benchmark]
        public void SqlStreamStoreLoad()
        {
            EventStoreLoad();
        }


        //[Benchmark]
        public void GetEventStoreLoad()
        {
            EventStoreLoad();
        }


        public void EventStoreSave()
        {
            using (var scope = _container.CreateScope())
            {
                var eventStore = scope.ServiceProvider.GetService<IEventStore>();
                eventStore.AppendEventsToStreamAsync(Guid.NewGuid().ToString(), new List<IEvent> { GetATestEvent() }, null, CancellationToken.None).Wait();
            }
        }

        public void EventStoreLoad()
        {
            using (var scope = _container.CreateScope())
            {
                var eventStore = scope.ServiceProvider.GetService<IEventStore>();
                eventStore.GetEventsFromStreamAsync(_loadTestStream, null, CancellationToken.None).Wait();
            }
        }


        private void SeedEventRepository(string stream)
        {
            var events = Enumerable.Range(0, 100)
                .Select(r => GetATestEvent());

            using (var scope = _container.CreateScope())
            {
                var eventStore = scope.ServiceProvider.GetService<IEventStore>();
                eventStore.AppendEventsToStreamAsync(stream, events, null, CancellationToken.None).Wait();
            }
        }



        private static void MigrateNBBEventStore(bool forceMultiTenant)
        {
            new AdoNetEventStoreDatabaseMigrator(forceMultiTenant).ReCreateDatabaseObjects(default).Wait();
        }

        private static void MigrateSqlStreamStore()
        {
            var migrator = new SqlStreamStoreMigrator();
            migrator.EnsureDatabaseDeleted().Wait();
            migrator.MigrateDatabaseToLatestVersion().Wait();
        }

        private static TestEvent GetATestEvent()
        {
            return new TestEvent
            {
                EventId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid()
            };
        }

        private static IServiceProvider BuildServiceProvider(Action<ServiceCollection, IConfiguration> addEventStoreAction)
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

            addEventStoreAction(services, configuration);

            var container = services.BuildServiceProvider();
            return container;
        }


        private class TenantContextAccessorMock : ITenantContextAccessor
        {
            private readonly TenantContext _tenantContext = new TenantContext(new Tenant(Guid.NewGuid(), null, false));
            public TenantContext TenantContext { get => _tenantContext; set => throw new NotImplementedException(); }

            public TenantContextFlow ChangeTenantContext(TenantContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
