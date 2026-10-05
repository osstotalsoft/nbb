using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Kafka.IntegrationTests
{
    public class KafkaTransportTests
    {
        [Fact]
        public async Task Test_unsubscribe_with_dispose()
        {
            var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();
            var sub = await msgBus.SubscribeAsync(_e => Task.CompletedTask, MessagingSubscriberOptions.Default with { TopicName = "MyTestTopic", Transport = SubscriptionTransportOptions.RequestReply });
            sub.Dispose();
        }

        [Fact]
        public async Task Test_unsubscribe_with_cancel_and_dispose()
        {
            var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();
            var cts = new CancellationTokenSource();
            var sub = await msgBus.SubscribeAsync(_e => Task.CompletedTask, MessagingSubscriberOptions.Default with { TopicName = "MyTestTopic", Transport = SubscriptionTransportOptions.RequestReply }, cts.Token);
            cts.Cancel();
            sub.Dispose();
        }

        [Fact]
        public async Task Test_publish_then_subscribe_round_trip()
        {
            var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();
            var received = new TaskCompletionSource<bool>();
            var topic = "MyTestTopic-" + Guid.NewGuid();
            await msgBus.PublishAsync(new TestPayload { Text = "MyTestMessage" }, MessagingPublisherOptions.Default with { TopicName = topic });
            var sub = await msgBus.SubscribeAsync<TestPayload>(
                _e => { received.TrySetResult(true); return Task.CompletedTask; },
                MessagingSubscriberOptions.Default with { TopicName = topic, Transport = SubscriptionTransportOptions.StreamProcessor with { DeliverNewMessagesOnly = false } });
            await received.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));
            // the TCS is completed from inside the transport poll task: yield so Dispose does not run on the poll thread
            await Task.Yield();
            sub.Dispose();
        }

        private class TestPayload
        {
            public string Text;
        }

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddMessageBus().AddKafkaTransport(configuration);

            return services.BuildServiceProvider();

        }
    }
}
