using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.AdoNet.Internal;

namespace NBB.EventStore.AdoNet
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
