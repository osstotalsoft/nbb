// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Migrations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NBB.Data.Abstractions;
using NBB.Data.EventSourcing;
using NBB.Domain;
using System.Reflection;
using MediatR;

namespace TheBenchmarks
{
    //[SimpleJob(runStrategy:RunStrategy.ColdStart, launchCount: 2, warmupCount: 0, targetCount: 5)]
    //[SimpleJob(runStrategy:RunStrategy.ColdStart, launchCount: 10, warmupCount: 0, targetCount: 1)]
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 10)]
    [RPlotExporter, RankColumn]
    public class EventSourcedRepositoryBenchmark
    {
        private IServiceProvider _container;
        private TestAggregate _loadedAggregate;
        private readonly Guid _loadTestAggregateId = Guid.NewGuid();

        private const int _snapshotFrequency = 10;
        private const bool _useJunkData = false;
        private const int _junkDataAggregates = 100;
        private const int _junkDataEventsPerAggregate = 1000;

        [Params(10, 100, 1000)]
        public int NumberOfEvents { get; set; }

        public void GlobalSetup(bool withSnapshot)
        {
            MigrateNBBEventStore();

            _container = BuildServiceProvider(services =>
            {
                services.AddEventSourcingDataAccess((sp, builder) => builder.Options.DefaultSnapshotVersionFrequency = _snapshotFrequency);

                if (withSnapshot)
                    services.AddEventSourcedRepository<TestSnapshotAggregate>();
                else
                    services.AddEventSourcedRepository<TestAggregate>();
            });

            if (withSnapshot)
                SeedEventRepository<TestSnapshotAggregate>(_useJunkData);
            else
                SeedEventRepository<TestAggregate>(_useJunkData);
        }

        [GlobalSetup(Target = nameof(LoadAggregateWithoutSnapshot))]
        public void GlobalSetupLoadAggregateWithoutSnapshot()
        {
            GlobalSetup(false);
            //LoadAggregate<TestAggregate>();
            //TryLoadRandomAggregate<TestAggregate>();
        }

        [GlobalSetup(Target = nameof(LoadAndSaveAggregateWithoutSnapshot))]
        public void GlobalSetupLoadAndSaveAggregateWithoutSnapshot()
        {
            GlobalSetup(false);
            //LoadAndSaveAggregateWithoutSnapshot();
        }

        [GlobalSetup(Target = nameof(SaveAggregateWithoutSnapshot))]
        public void GlobalSetupSaveAggregateWithoutSnapshot()
        {
            GlobalSetup(false);
        }

        [GlobalSetup(Target = nameof(LoadAggregateWithSnapshot))]
        public void GlobalSetupLoadAggregateWithSnapshot()
        {
            GlobalSetup(true);
            //LoadAggregate<TestSnapshotAggregate>();
            //TryLoadRandomAggregate<TestSnapshotAggregate>();
        }

        [GlobalSetup(Target = nameof(LoadAndSaveAggregateWithSnapshot))]
        public void GlobalSetupLoadAndSaveAggregateWithSnapshot()
        {
            GlobalSetup(true);
            //LoadAndSaveAggregateWithSnapshot();
        }

        [GlobalSetup(Target = nameof(SaveAggregateWithSnapshot))]
        public void GlobalSetupSaveAggregateWithSnapshot()
        {
            GlobalSetup(true);
        }

        //[Benchmark]
        public void LoadAggregateWithoutSnapshot()
        {
            LoadAggregate<TestAggregate>();
        }

        //[Benchmark]
        public void LoadAggregateWithSnapshot()
        {
            LoadAggregate<TestSnapshotAggregate>();
        }

        [Benchmark]
        public void LoadAndSaveAggregateWithSnapshot()
        {
            var aggregate = LoadAggregate<TestSnapshotAggregate>();
            SaveAggregate(aggregate);
        }

        [Benchmark]
        public void LoadAndSaveAggregateWithoutSnapshot()
        {
            var aggregate = LoadAggregate<TestAggregate>();
            SaveAggregate(aggregate);
        }

        //[Benchmark]
        public void SaveAggregateWithSnapshot()
        {
            SaveAggregate((TestSnapshotAggregate)_loadedAggregate);
        }

        //[Benchmark]
        public void SaveAggregateWithoutSnapshot()
        {
            SaveAggregate(_loadedAggregate);
        }

        public TAggregateRoot LoadAggregate<TAggregateRoot>() where TAggregateRoot : TestAggregate
        {
            using var scope = _container.CreateScope();
            var repository = scope.ServiceProvider.GetService<IEventSourcedRepository<TAggregateRoot>>();
            var aggregate = repository.GetByIdAsync(_loadTestAggregateId, default).GetAwaiter().GetResult();

            if (aggregate?.AggregateId == default(Guid))
                aggregate.AggregateId = _loadTestAggregateId;

            return aggregate;
        }

        public void TryLoadRandomAggregate<TAggregateRoot>() where TAggregateRoot : TestAggregate
        {
            using var scope = _container.CreateScope();
            var repository = scope.ServiceProvider.GetService<IEventSourcedRepository<TAggregateRoot>>();
            var aggregate = repository.GetByIdAsync(Guid.NewGuid(), default).GetAwaiter().GetResult();
        }

        public void SaveAggregate<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : TestAggregate, new()
        {
            using var scope = _container.CreateScope();
            var repository = scope.ServiceProvider.GetService<IEventSourcedRepository<TAggregateRoot>>();
            aggregate.DoAction($"Event {Guid.NewGuid()}");
            aggregate.DoAction($"Event {Guid.NewGuid()}");
            repository.SaveAsync(aggregate).GetAwaiter().GetResult();
            _loadedAggregate = aggregate;
        }

        private void SeedEventRepository<TAggregateRoot>(bool useJunkData = false) where TAggregateRoot : TestAggregate, new()
        {
            if (useJunkData)
            {
                SeedJunkData<TAggregateRoot>();
            }

            SeedAggregates<TAggregateRoot>(NumberOfEvents, _loadTestAggregateId);
        }

        private void SeedJunkData<TAggregateRoot>() where TAggregateRoot : TestAggregate, new()
        {
            var aggregateIds = Enumerable.Range(0, _junkDataAggregates).Select(x => Guid.NewGuid()).ToArray();

            SeedAggregates<TAggregateRoot>(_junkDataEventsPerAggregate, aggregateIds);
        }

        private void SeedAggregates<TAggregateRoot>(int eventNo, params Guid[] aggregateIds) where TAggregateRoot : TestAggregate, new()
        {
            using var scope = _container.CreateScope();
            var repository = scope.ServiceProvider.GetService<IEventSourcedRepository<TAggregateRoot>>();

            foreach (var aggregateId in aggregateIds)
            {
                var aggregate = new TAggregateRoot() { AggregateId = aggregateId };
                for (int i = 0; i < eventNo; i++)
                {
                    aggregate.DoAction($"Value {i + 1}");

                    if ((i + 1) % _snapshotFrequency == 0)
                        repository.SaveAsync(aggregate).GetAwaiter().GetResult();
                }

                repository.SaveAsync(aggregate).GetAwaiter().GetResult();

                aggregate.DoAction($"AdditionlValue 1");
                aggregate.DoAction($"AdditionlValue 2");
                aggregate.DoAction($"AdditionlValue 3");

                repository.SaveAsync(aggregate).GetAwaiter().GetResult();

                if (aggregateId == _loadTestAggregateId)
                    _loadedAggregate = aggregate;
            }
        }

        private static void MigrateNBBEventStore()
        {
            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(default).Wait();
        }

        private static TestEvent GetATestEvent()
        {
            return new TestEvent("Lorem ipsum");
        }

        private static IServiceProvider BuildServiceProvider(Action<ServiceCollection> addEventStoreAction)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
            }

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();

            services.AddMediatR(typeof(Program).Assembly);

            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithAdoNetEventRepository();

            addEventStoreAction(services);

            var container = services.BuildServiceProvider();
            return container;
        }

        public record TestEvent(string Prop);

        public class TestAggregate : EventSourcedAggregateRoot<Guid>
        {
            public Guid AggregateId { get; set; }

            protected List<string> State { get; set; } = new List<string>();

            public string Prop1 { get; protected set; }

            public override Guid GetIdentityValue()
            {
                return AggregateId;
            }

            public void DoAction(string prop)
            {
                Emit(new TestEvent(prop));
            }

            private void Apply(TestEvent testEvent)
            {
                if (State.Count > 100)
                    State.Clear();

                State.Add(testEvent.Prop);
            }
        }

        public class TestSnapshotAggregate : TestAggregate, IMementoProvider<TestSnapshot>
        {
            public void SetMemento(TestSnapshot memento)
            {
                AggregateId = memento.AggregateId;
                State = memento.State.ToList();
            }

            public TestSnapshot CreateMemento()
            {
                return new TestSnapshot(AggregateId, State.ToList());
            }

            void IMementoProvider.SetMemento(object memento) => SetMemento((TestSnapshot)memento);
            object IMementoProvider.CreateMemento() => CreateMemento();
        }


        public class TestSnapshot
        {
            public Guid AggregateId { get; }
            public IEnumerable<string> State { get; }

            public TestSnapshot(Guid aggregateId, IEnumerable<string> state)
            {
                AggregateId = aggregateId;
                State = state;
            }
        }
    }
}
