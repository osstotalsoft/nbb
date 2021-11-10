// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Noop;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNoopTransport(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessagingTransport, NoopMessagingTransport>();
            services.AddSingleton<ITransportMonitor, NoopTransportMonitor>();

            return services;
        }
    }
}
