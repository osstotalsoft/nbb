// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.InProcessMessaging.Tests
{
    public class InProcessMessagingTests
    {
        [Fact]
        public async Task subsequent_msg_bus_subscriptions_for_same_topic()
        {
            //Arrange
            using var sp = BuildServiceProvider();
            var msgBus = sp.GetRequiredService<IMessageBus>();

            Func<Task> subscribe = async () =>
            {
                using var sub = await msgBus.SubscribeAsync(e => Task.CompletedTask, options: MessagingSubscriberOptions.Default with { TopicName = "SomeTopic" });
            };

            //Act
            await subscribe();
            await subscribe(); //should not throw already subscribed

        }

        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddMessageBus().AddInProcessTransport();

            return services.BuildServiceProvider();

        }
    }
}
