﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.JetStream;
using NBB.Messaging.JetStream.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNatsTransport(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NatsOptions>(configuration.GetSection("Messaging").GetSection("Nats"));
            services.AddSingleton<NatsConnectionProvider>();
            services.AddSingleton<IMessagingTransport, NatsMessagingTransport>();
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<NatsConnectionProvider>());

            return services;
        }
    }
}
