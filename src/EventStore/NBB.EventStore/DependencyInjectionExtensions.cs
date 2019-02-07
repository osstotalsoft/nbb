using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Infrastructure;
using NBB.EventStore.Internal;
using Newtonsoft.Json;

namespace NBB.EventStore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, Action<IServiceProvider, EventStoreOptionsBuilder> optionsAction = null)
        {
            services.AddSingleton<IEventStore, EventStore>();
            services.AddSingleton<ISnapshotStore, SnapshotStore>();

            services.Add(new ServiceDescriptor(typeof(EventStoreOptions),
                serviceProvider =>
                {
                    var builder = new EventStoreOptionsBuilder();
                    optionsAction?.Invoke(serviceProvider, builder);
                    return builder.Options;
                }, ServiceLifetime.Singleton));

            return services;

        }

        public static IServiceCollection WithNewtownsoftJsonEventStoreSeserializer(this IServiceCollection services, IEnumerable<JsonConverter> converters = null)
        {
            services.AddSingleton<IEventStoreSerDes>(sp => new NewtonsoftJsonEventStoreSerDes(converters));

            return services;

        }
    }
}
