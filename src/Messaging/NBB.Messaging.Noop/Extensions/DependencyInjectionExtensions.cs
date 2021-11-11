using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Noop.Internal;

namespace NBB.Messaging.Noop
{
    public static class DependencyInjectionExtensions
    {
        public static void AddNoopMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton(typeof(IMessageBusSubscriber<>), typeof(NoopMessageBusSubscriber<>));
            services.AddTransient<IMessagingTopicSubscriber, NoopMessagingTopicSubscriber>();
            services.AddTransient<IMessagingTopicPublisher, NoopMessagingTopicPublisher>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
        }
    }
}
