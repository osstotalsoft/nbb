using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Rusi.IntegrationTests
{
    public class RusiTransportTests
    {
        //[Fact]
        public async Task Test_unsubscribe_with_dispose()
        {
            var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();
            var sub = await msgBus.SubscribeAsync(_e => Task.CompletedTask, MessagingSubscriberOptions.Default with { TopicName = "MyTestTopic", Transport = SubscriptionTransportOptions.RequestReply });
            sub.Dispose();
        }

        //[Fact]
        public async Task Test_unsubscribe_with_cancel_and_dispose()
        {
            var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();
            var cts = new CancellationTokenSource();
            var sub = await msgBus.SubscribeAsync(_e => Task.CompletedTask, MessagingSubscriberOptions.Default with { TopicName = "MyTestTopic", Transport = SubscriptionTransportOptions.RequestReply }, cts.Token);
            cts.Cancel();
            sub.Dispose();
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
            services.AddMessageBus(configuration).AddRusiTransport(configuration);

            return services.BuildServiceProvider();

        }
    }
}
