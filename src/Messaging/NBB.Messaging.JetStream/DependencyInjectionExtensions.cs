// Copyright (c) TotalSoft.
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
            services.Configure<JetStreamOptions>(configuration.GetSection("Messaging").GetSection("JetStream"));
            services.AddSingleton<JetStreamConnectionProvider>();
            services.AddSingleton<IMessagingTransport, JetStreamMessagingTransport>();
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<JetStreamConnectionProvider>());

            return services;
        }
    }
}
