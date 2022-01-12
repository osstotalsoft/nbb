// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using NBB.Messaging.InProcessMessaging.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddInProcessTransport(this IServiceCollection services)
        {
            services.AddSingleton<IStorage, Storage>();

            services.AddSingleton<InProcessMessagingTransport>();
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<InProcessMessagingTransport>());
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<InProcessMessagingTransport>());
        }
    }
}
