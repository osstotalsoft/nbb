using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Builder;

namespace NBB.Messaging.Host
{
    public static class ServiceCollectionExtensions
    {
        public static MessagingHostBuilder AddMessagingHost(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<MessagingContextAccessor>();
            serviceCollection.Decorate<IMessageBusPublisher, MessagingContextBusPublisherDecorator>();

            return new MessagingHostBuilder(serviceCollection);
        }

        public static void AddMessageBusMediator(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMessageBusMediator, MessageBusMediator>();
        }
    }
}
