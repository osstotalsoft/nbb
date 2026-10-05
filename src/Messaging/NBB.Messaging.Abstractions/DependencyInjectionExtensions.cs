// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
            services.AddSingleton<IDeadLetterQueue, DefaultDeadLetterQueue>();

            ApplyDeferredConfigurations(services);

            return services;
        }

        /// <summary>
        /// Configures the message bus services (for example decorates <see cref="IMessageBusPublisher"/>),
        /// regardless of whether <see cref="AddMessageBus"/> has been called yet.
        /// If the message bus is already registered the configuration is applied immediately,
        /// otherwise it is applied when <see cref="AddMessageBus"/> is called.
        /// If the message bus is never registered the configuration is not applied.
        /// </summary>
        public static IServiceCollection ConfigureMessageBusServices(this IServiceCollection services, Action<IServiceCollection> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            if (services.Any(d => d.ServiceType == typeof(IMessageBusPublisher)))
            {
                configure(services);
            }
            else
            {
                services.AddSingleton(new MessageBusServicesConfiguration(configure));
            }

            return services;
        }

        private static void ApplyDeferredConfigurations(IServiceCollection services)
        {
            var deferred = services
                .Where(d => d.ServiceType == typeof(MessageBusServicesConfiguration))
                .ToList();

            foreach (var descriptor in deferred)
            {
                // applied once: a later AddMessageBus call does not apply them again
                services.Remove(descriptor);
                ((MessageBusServicesConfiguration)descriptor.ImplementationInstance).Configure(services);
            }
        }
    }
}
