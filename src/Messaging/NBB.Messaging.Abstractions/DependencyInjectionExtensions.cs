// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();
            services.AddSingleton<IMessageBusSubscriber, MessageBusSubscriber>();
            services.AddSingleton<ITopicRegistry, DefaultTopicRegistry>();
            services.AddSingleton<IMessageSerDes, NewtonsoftJsonMessageSerDes>();
            services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
            services.AddSingleton<IMessageBus, MessageBus>();
            services.AddSingleton<IDeadLetterQueue, DefaultDeadLetterQueue>();
            services.Configure<MessagingOptions>(configuration.GetSection("Messaging"));

            return services;
        }
    }
}
