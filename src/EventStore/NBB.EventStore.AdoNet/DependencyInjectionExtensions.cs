// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore;
using NBB.EventStore.AdoNet.Internal;
using NBB.EventStore.AdoNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection WithAdoNetEventRepository(this IServiceCollection services)
        {
            services.AddSingleton<IEventRepository, AdoNetEventRepository>();
            services.AddSingleton<ISnapshotRepository, AdoNetSnapshotRepository>();
            services.AddSingleton<Scripts>();
            
            return services;
        }
    }
}
