// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.EventStore.AdoNet.Multitenancy;
using NBB.EventStore.AdoNet.Multitenancy.Internal;
using NBB.MultiTenancy.Abstractions.Configuration;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MultiTenantEventStoreAdoNetBuilderExtensions
{
    public static EventStoreAdoNetOptionsBuilder FromTenantConfiguration(this EventStoreAdoNetOptionsBuilder b, string name)
        => b.From<ITenantConfiguration>((c, o) => o.ConnectionString = c.GetConnectionString(name));
}

public static class MultiTenantAdoNetEventStoreOptionsBuilderExtensions
{
    public static EventStoreOptionsBuilder UseMultiTenantAdoNetEventRepository(this EventStoreOptionsBuilder b, Action<EventStoreAdoNetOptionsBuilder> optionsAction) => ((IEventStoreOptionsBuilder)b).AdExtension(services =>
    {
        services.AddScoped<IEventRepository, AdoNetMultiTenantEventRepository>();
        services.AddScoped<ISnapshotRepository, AdoNetMultitenantSnapshotRepository>();
        services.AddSingleton<NBB.EventStore.AdoNet.Internal.Scripts, Scripts>();

        var b = new EventStoreAdoNetOptionsBuilder();
        optionsAction?.Invoke(b);
        services.AddOptions<EventStoreAdoNetOptions>().Configure<IServiceProvider>(((IEventStoreAdoNetOptionsBuilder)b).Configure);
    });
}
