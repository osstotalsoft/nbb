using System;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka.Infrastructure;

namespace NBB.Messaging.Kafka
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, Action<IServiceProvider,  KafkaMessagingOptionsBuilder> optionsAction = null)
        {
            services.AddSingleton(typeof(IMessageBusPublisher), typeof(MessageBusPublisher));
            services.AddSingleton(typeof(IMessageBusSubscriber), typeof(MessageBusSubscriber));
            services.AddTransient<IMessagingTopicSubscriber, KafkaMessagingTopicSubscriber>();
            services.AddTransient<IMessagingTopicPublisher, KafkaMessagingTopicPublisher>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
            

            services.Add(new ServiceDescriptor(typeof(KafkaMessagingOptions),
                serviceProvider =>
                {
                    var builder = new KafkaMessagingOptionsBuilder();
                    optionsAction?.Invoke(serviceProvider, builder);
                    return builder.Options;
                }, ServiceLifetime.Singleton));

            return services;
        }
    }
}
