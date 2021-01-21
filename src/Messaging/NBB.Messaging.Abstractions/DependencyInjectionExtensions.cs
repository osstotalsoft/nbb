using Microsoft.Extensions.DependencyInjection;

namespace NBB.Messaging.Abstractions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMessagingDefaults(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton(typeof(IMessageBusSubscriber<>), typeof(MessageBusSubscriber<>));
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
            services.AddTransient<IMessagingTopicSubscriber, MessagingTopicSubscriber>();
            services.AddTransient<IMessagingTopicPublisher, MessagingTopicPublisher>();
        }
    }
}