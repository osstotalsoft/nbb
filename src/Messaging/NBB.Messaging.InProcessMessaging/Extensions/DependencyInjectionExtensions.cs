using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.InProcessMessaging.Internal;

namespace NBB.Messaging.InProcessMessaging.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddInProcessMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IStorage, Storage>();
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton(typeof(IMessageBusSubscriber<>), typeof(MessageBusSubscriber<>));
            services.AddTransient<IMessagingTopicSubscriber, InProcessMessagingTopicSubscriber>();
            services.AddTransient<IMessagingTopicPublisher, InProcessMessagingTopicPublisher>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
        }
    }
}
