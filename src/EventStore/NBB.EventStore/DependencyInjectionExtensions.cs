// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddEventStore(this IServiceCollection services, Action<EventStoreOptionsBuilder> optionsAction)
    {
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<ISnapshotStore, SnapshotStore>();

        var b = new EventStoreOptionsBuilder();
        optionsAction?.Invoke(b);
        ((IEventStoreOptionsBuilder)b).AddServices(services);

        return services;
    }
}


public interface IEventStoreOptionsBuilder
{
    EventStoreOptionsBuilder Add(Action<IServiceCollection> extension);
    void AddServices(IServiceCollection services);
}

public class EventStoreOptionsBuilder: IEventStoreOptionsBuilder
{
    private readonly List<Action<IServiceCollection>> extensions = new();

    EventStoreOptionsBuilder IEventStoreOptionsBuilder.Add(Action<IServiceCollection> extension)
    {
        extensions.Add(extension);
        return this;
    }

    void IEventStoreOptionsBuilder.AddServices(IServiceCollection services) =>
        extensions.ForEach(e => e(services));

    public EventStoreOptionsBuilder UseNewtownsoftJson(params JsonConverter[] converters)
        => ((IEventStoreOptionsBuilder)this).Add(services => services.AddSingleton<IEventStoreSerDes>(sp => new NewtonsoftJsonEventStoreSerDes(converters)));
}
