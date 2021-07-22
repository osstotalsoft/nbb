﻿using NBB.Messaging.Abstractions;
using NBB.Messaging.InProcessMessaging.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddInProcessTransport(this IServiceCollection services)
        {
            services.AddSingleton<IStorage, Storage>();
            services.AddSingleton<IMessagingTransport, InProcessMessagingTransport>();
        }
    }
}
