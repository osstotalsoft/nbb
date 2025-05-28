// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka;
using NBB.Messaging.Kafka.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddKafkaTransport(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaOptions>(configuration.GetSection("Messaging").GetSection("Kafka"));
            services.AddSingleton<KafkaConnectionProvider>();
            services.AddSingleton<KafkaMessagingTransport>();
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<KafkaMessagingTransport>());
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<KafkaMessagingTransport>());
            return services;
        }
    }
}

