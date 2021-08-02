using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Infrastructure;
using NBB.EventStore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using AbstractionsEventStoreOptions = NBB.EventStore.Abstractions.EventStoreOptions;
using InfrastructureEventStoreOptions = NBB.EventStore.Infrastructure.EventStoreOptions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, Action<IServiceProvider, EventStoreOptionsBuilder> optionsAction = null)
        {
            services.AddSingleton<IEventStore, EventStore>();
            services.AddSingleton<ISnapshotStore, SnapshotStore>();

            services.AddSingleton<IConfigureOptions<AbstractionsEventStoreOptions>, ConfigureEventStoreOptions>();

            services.Add(new ServiceDescriptor(typeof(InfrastructureEventStoreOptions),
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

    internal class ConfigureEventStoreOptions : IConfigureOptions<AbstractionsEventStoreOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureEventStoreOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(AbstractionsEventStoreOptions options) =>
            _configuration.GetSection("EventStore").GetSection("NBB").Bind(options);
    }
}