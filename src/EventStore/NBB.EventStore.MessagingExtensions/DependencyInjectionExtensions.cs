using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.Abstractions;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Messaging.Abstractions;
using System;

namespace NBB.EventStore.MessagingExtensions
{
    public static class DependencyInjectionExtensions
    {
        public static void WithMessagingExtensions(this IServiceCollection services, Action<MessagingSubscriberOptionsBuilder> subscriberOptionsBuilder = null)
        {
            services.Decorate<IEventStore, MessagingEventStoreDecorator>();
            services.AddTransient(sp =>
            {
                var builder = new MessagingSubscriberOptionsBuilder(sp.GetService<MessagingSubscriberOptions>());
                subscriberOptionsBuilder?.Invoke(builder);

                return (IEventStoreSubscriber)ActivatorUtilities.CreateInstance(sp, typeof(MessagingEventStoreSubscriber), builder.Options);
            });
            services.AddSingleton<MessagingContextAccessor>();
            services.AddSingleton<MessagingTopicResolver>();
        }
    }
}
