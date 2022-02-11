// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore;
using NBB.EventStore.AdoNet.Internal;
using NBB.EventStore.AdoNet;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public interface IEventStoreAdoNetOptionsBuilder
{
    void Configure(EventStoreAdoNetOptions options, IServiceProvider sp);
}
public class EventStoreAdoNetOptionsBuilder: IEventStoreAdoNetOptionsBuilder
{
    private readonly List<Action<IServiceProvider, EventStoreAdoNetOptions>> mutations = new();

    void IEventStoreAdoNetOptionsBuilder.Configure(EventStoreAdoNetOptions options, IServiceProvider sp)
        => mutations.ForEach(f => f(sp, options));

    public EventStoreAdoNetOptionsBuilder From(Action<IServiceProvider, EventStoreAdoNetOptions> a)
    {
        mutations.Add(a);
        return this;
    }

    public EventStoreAdoNetOptionsBuilder From<TDep>(Action<TDep, EventStoreAdoNetOptions> a) => From((sp, o) =>
    {
        var d = sp.GetRequiredService<TDep>();
        a(d, o);
    });

    public EventStoreAdoNetOptionsBuilder From<TDep1, TDep2>(Action<TDep1, TDep2, EventStoreAdoNetOptions> a) => From((sp, o) =>
    {
        var dep1 = sp.GetRequiredService<TDep1>();
        var dep2 = sp.GetRequiredService<TDep2>();
        a(dep1, dep2, o);
    });

    public EventStoreAdoNetOptionsBuilder From<TDep1, TDep2, TDep3>(Action<TDep1, TDep2, TDep3, EventStoreAdoNetOptions> a) => From((sp, o) =>
    {
        var dep1 = sp.GetRequiredService<TDep1>();
        var dep2 = sp.GetRequiredService<TDep2>();
        var dep3 = sp.GetRequiredService<TDep3>();
        a(dep1, dep2, dep3, o);
    });

    public EventStoreAdoNetOptionsBuilder FromConfiguration()
        => From<IConfiguration>((c, o) => c.GetSection("EventStore").GetSection("NBB").Bind(o));
}

public static class EventStoreOptionsBuilderExtensions
{
    public static EventStoreOptionsBuilder UseAdoNetEventRepository(this EventStoreOptionsBuilder b, Action<EventStoreAdoNetOptionsBuilder> optionsAction) => ((IEventStoreOptionsBuilder)b).AdExtension(services =>
    {
        services.AddScoped<IEventRepository, AdoNetEventRepository>();
        services.AddScoped<ISnapshotRepository, AdoNetSnapshotRepository>();
        services.AddSingleton<Scripts>();

        var b = new EventStoreAdoNetOptionsBuilder();
        optionsAction?.Invoke(b);
        services.AddOptions<EventStoreAdoNetOptions>().Configure<IServiceProvider>(((IEventStoreAdoNetOptionsBuilder)b).Configure);
    });
}

