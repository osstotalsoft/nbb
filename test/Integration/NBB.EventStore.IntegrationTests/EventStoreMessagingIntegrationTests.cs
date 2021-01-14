using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Host;
using NBB.EventStore.InMemory;
using NBB.EventStore.MessagingExtensions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.InProcessMessaging.Extensions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.EventStore.IntegrationTests
{
    public class EventStoreMessagingIntegrationTests : IClassFixture<EnvironmentFixture>
    {
        [Fact]
        public async Task EventStore_Messaging_Publish_Subscribe()
        {
            Guid eventId = Guid.NewGuid();
            object hostMessageReceived = null;
            var hostMessageReceivedEvent = new ManualResetEventSlim();

            void HostMessageReceived(object @event)
            {
                hostMessageReceived = @event;
                hostMessageReceivedEvent.Set();
            }

            var container = BuildMessagingServiceProvider(HostMessageReceived);
            var stream = Guid.NewGuid().ToString();

            using (var scope = container.CreateScope())
            {
                var host = scope.ServiceProvider.GetService<IHostedService>();
                try
                {
                    await host.StartAsync(CancellationToken.None);

                    var eventStore = scope.ServiceProvider.GetService<IEventStore>();
                    eventStore.AppendEventsToStreamAsync(stream, new[] {new TestEvent(eventId)}, null,
                        CancellationToken.None).Wait();

                    hostMessageReceivedEvent.Wait(5000);
                }
                finally
                {
                    await host.StopAsync(CancellationToken.None);
                }
            }

            hostMessageReceived.Should().NotBeNull();
            ((TestEvent)hostMessageReceived).EventId.Should().Be(eventId);
        }

        private static IServiceProvider BuildMessagingServiceProvider(Action<object> hostMessageReceived = null)
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
                .WithInMemoryEventRepository()
                .WithMessagingExtensions(builder =>
                {
                    builder.Options.SerDes.DeserializationType = DeserializationType.Dynamic;
                    builder.Options.SerDes.DynamicDeserializationScannedAssemblies = new[]
                    {
                        typeof(TestEventMessaging).Assembly
                    };
                });

            services.AddInProcessMessaging();

            services.AddEventStoreHost()
                .UsePipeline(config => config
                    .Use((e, ct, next) =>
                    {
                        hostMessageReceived?.Invoke(e);
                        return next();
                    })
                );

            var container = services.BuildServiceProvider();
            return container;
        }
    }

    public record TestEventMessaging;
}
