using System;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Builder;

namespace NBB.Messaging.Host
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingHost(this IServiceCollection serviceCollection, Action<MessagingHostBuilder> messagingHostBuilder)
        {
            serviceCollection.AddSingleton<MessagingContextAccessor>();
            serviceCollection.Decorate<IMessageBusPublisher, MessagingContextBusPublisherDecorator>();

            var builder = new MessagingHostBuilder(serviceCollection);
            messagingHostBuilder?.Invoke(builder);

            return serviceCollection;
        }
    }
}
