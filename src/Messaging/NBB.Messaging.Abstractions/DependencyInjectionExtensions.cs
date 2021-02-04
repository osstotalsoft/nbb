using Microsoft.Extensions.DependencyInjection;

namespace NBB.Messaging.Abstractions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton<IMessageBusSubscriber, MessageBusSubscriber>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
            services.AddSingleton<IMessageBus, MessageBus>();

            return services;
        }
    }
}