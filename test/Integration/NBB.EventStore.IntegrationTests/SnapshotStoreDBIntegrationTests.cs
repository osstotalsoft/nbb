using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Migrations;
using NBB.EventStore.AdoNet.Multitenancy;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.EventStore.IntegrationTests
{
    [Collection("EventStoreDB")]
    public class SnapshotStoreDbIntegrationTests : IClassFixture<EnvironmentFixture>
    {
        [Fact]
        public void Should_store_snapshot_thread_safe()
        {
            // Arrange
            PrepareDb();
            var container = BuildAdoRepoServiceProvider();
            var stream = Guid.NewGuid().ToString();
            const int streamVersion = 0;
            const int threadCount = 10;
            var concurrencyExceptionCount = 0;

            using (var scope = container.CreateScope())
            {
                var snapshotStore = scope.ServiceProvider.GetService<ISnapshotStore>();

                // Act
                Parallel.For(0, threadCount, _ =>
               {
                   try
                   {
                       snapshotStore.StoreSnapshotAsync(
                           new SnapshotEnvelope(
                               new TestSnapshot { Prop1 = "aaa", Prop2 = "bbb" }, streamVersion, stream),
                           CancellationToken.None
                       ).GetAwaiter().GetResult();
                   }
                   catch (ConcurrencyUnrecoverableException)
                   {
                       Interlocked.Increment(ref concurrencyExceptionCount);
                   }
               });
                var snapshot = snapshotStore.LoadSnapshotAsync(stream, CancellationToken.None).Result;

                // Assert
                snapshot.Should().NotBeNull();
                concurrencyExceptionCount.Should().Be(threadCount - 1);
            }
        }

        [Fact]
        public void Should_retrieve_snapshot_with_latest_version()
        {
            // Arrange
            PrepareDb();
            var container = BuildAdoRepoServiceProvider();
            var stream = Guid.NewGuid().ToString();
            const int threadCount = 10;

            using (var scope = container.CreateScope())
            {
                var snapshotStore = scope.ServiceProvider.GetService<ISnapshotStore>();

                // Act
                Parallel.For(0, threadCount, index =>
               {
                   snapshotStore.StoreSnapshotAsync(
                        new SnapshotEnvelope(
                            new TestSnapshot { Prop1 = "aaa", Prop2 = "bbb" }, index, stream),
                        CancellationToken.None
                    ).GetAwaiter().GetResult();

               });
                var snapshot = snapshotStore.LoadSnapshotAsync(stream, CancellationToken.None).Result;

                // Assert
                snapshot.Should().NotBeNull();
                snapshot.AggregateVersion.Should().Be(threadCount - 1);
            }
        }

        [Fact]
        public async Task Should_load_stored_snapshot()
        {
            //Arrange
            PrepareDb();
            var container = BuildAdoRepoServiceProvider();
            var stream = Guid.NewGuid().ToString();
            var snapshot = new TestSnapshot { Prop1 = "aaa", Prop2 = "bbb" };
            var snapshotEnvelope = new SnapshotEnvelope(snapshot, 1, stream);

            using (var scope = container.CreateScope())
            {
                //Act
                var snapshotStore = scope.ServiceProvider.GetService<ISnapshotStore>();

                await snapshotStore.StoreSnapshotAsync(snapshotEnvelope, CancellationToken.None);

                var loadedSnapshotEnvelope = await snapshotStore.LoadSnapshotAsync(stream, CancellationToken.None);

                //Assert
                loadedSnapshotEnvelope.Should().NotBeNull();
                loadedSnapshotEnvelope.Should().BeEquivalentTo(snapshotEnvelope);
            }
        }

        [Fact]
        public async Task Should_return_null_for_not_found_snapshot()
        {
            //Arrange
            PrepareDb();
            var container = BuildAdoRepoServiceProvider();
            var stream = Guid.NewGuid().ToString();

            using (var scope = container.CreateScope())
            {
                //Act
                var snapshotStore = scope.ServiceProvider.GetService<ISnapshotStore>();
                var loadedSnapshotEnvelope = await snapshotStore.LoadSnapshotAsync(stream, CancellationToken.None);

                //Assert
                loadedSnapshotEnvelope.Should().BeNull();
            }
        }

        private static IServiceProvider BuildAdoRepoServiceProvider()
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

            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithAdoNetEventRepository();

            services.AddMultitenancy(configuration)
                .AddSingleton(Mock.Of<ITenantContextAccessor>(x =>
                            x.TenantContext == new TenantContext(new Tenant(Guid.NewGuid(), null))))
                .WithMultiTenantAdoNetEventRepository();

            var container = services.BuildServiceProvider();
            return container;
        }

        private static void PrepareDb()
        {
            new AdoNetEventStoreDatabaseMigrator(isTestHost: true).ReCreateDatabaseObjects(null).Wait();
        }
    }

    public class TestSnapshot
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }
}
