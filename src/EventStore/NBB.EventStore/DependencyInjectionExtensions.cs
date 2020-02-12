using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Infrastructure;
using NBB.EventStore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.EventStore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, Action<IServiceProvider, EventStoreOptionsBuilder> optionsAction = null)
        {
            services.AddSingleton<IEventStore, EventStore>();
            services.AddSingleton<ISnapshotStore, SnapshotStore>();

            services.AddSingleton<IConfigureOptions<Abstractions.EventStoreOptions>, ConfigureEventStoreOptions>();

            services.Add(new ServiceDescriptor(typeof(Infrastructure.EventStoreOptions),
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

    internal class ConfigureEventStoreOptions : IConfigureOptions<Abstractions.EventStoreOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureEventStoreOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(Abstractions.EventStoreOptions options)
        {
            var connectionString = _configuration.GetSection("EventStore").GetSection("NBB")["ConnectionString"];
            options.ConnectionString = connectionString;
        }
    }
}