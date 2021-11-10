// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using NBB.Messaging.Noop;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNoopTransport(this IServiceCollection services)
        {
            services.AddSingleton<IMessagingTransport, NoopMessagingTransport>();
            services.AddSingleton<ITransportMonitor, NoopTransportMonitor>();

            return services;
        }
    }
}
