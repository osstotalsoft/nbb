// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore;
using NBB.EventStore.AdoNet.Multitenancy;
using NBB.EventStore.AdoNet.Multitenancy.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection WithMultiTenantAdoNetEventRepository(this IServiceCollection services)
        {
            services.AddSingleton<IEventRepository, AdoNetMultiTenantEventRepository>();
            services.AddSingleton<ISnapshotRepository, AdoNetMultitenantSnapshotRepository>();
            services.AddSingleton<NBB.EventStore.AdoNet.Internal.Scripts, Scripts>();

            return services;
        }
    }
}
