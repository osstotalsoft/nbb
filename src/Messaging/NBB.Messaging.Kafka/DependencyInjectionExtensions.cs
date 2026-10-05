// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddKafkaTransport(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<KafkaOptions>()
                .Bind(configuration.GetSection("Messaging").GetSection("Kafka"))
                .Validate(options => !string.IsNullOrEmpty(options.BootstrapServers),
                    "missing bootstrap_servers")
                .Validate(options => !string.IsNullOrEmpty(options.GroupId),
                    "missing group_id");

            services.AddSingleton<IProducer<byte[], byte[]>>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>();
                return new ProducerBuilder<byte[], byte[]>(
                    new ProducerConfig
                    {
                        BootstrapServers = kafkaOptions.Value.BootstrapServers,
                        Acks = Acks.All,
                    }).Build();
            });

            services.AddSingleton<KafkaConsumerFactory>(sp =>
                new KafkaConsumerFactoryImpl(sp.GetRequiredService<IOptions<KafkaOptions>>()));

            services.AddSingleton<KafkaMessagingTransport>(sp =>
                new KafkaMessagingTransport(
                    sp.GetRequiredService<IProducer<byte[], byte[]>>(),
                    sp.GetRequiredService<KafkaConsumerFactory>()));

            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<KafkaMessagingTransport>());
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<KafkaMessagingTransport>());

            return services;
        }
    }
}
