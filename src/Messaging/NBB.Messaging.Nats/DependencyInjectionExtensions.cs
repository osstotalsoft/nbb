using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;

namespace NBB.Messaging.Nats
{
    public static class DependencyInjectionExtensions
    {
        public static void AddNatsMessaging(this IServiceCollection services)
        {
            services.AddSingleton<StanConnectionProvider>();
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton(typeof(IMessageBusSubscriber<>), typeof(MessageBusSubscriber<>));
            services.AddTransient<IMessagingTopicSubscriber, NatsMessagingTopicSubscriber>();
            services.AddTransient<IMessagingTopicPublisher, NatsMessagingTopicPublisher>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
        }
    }
}
