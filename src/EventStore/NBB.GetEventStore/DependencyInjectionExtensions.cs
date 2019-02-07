using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.Abstractions;
using NBB.GetEventStore.Internal;

namespace NBB.GetEventStore
{
    public static class DependencyInjectionExtensions
    {
        public static void AddGetEventStore(this IServiceCollection services)
        {
            services.AddScoped<IEventStore, GetEventStoreClient>();
            services.AddScoped<ISnapshotStore, NullSnapshotStore>();
            services.AddSingleton<ISerDes, SerDes>();
            services.AddTransient<IEventStoreSubscriber, GetEventStoreSubscriber>();

        }
    }
}
