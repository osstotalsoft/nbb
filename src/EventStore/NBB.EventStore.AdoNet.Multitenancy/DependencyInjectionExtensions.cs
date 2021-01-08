using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.AdoNet.Multitenancy.Internal;

namespace NBB.EventStore.AdoNet.Multitenancy
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection WithMultiTenantAdoNetEventRepository(this IServiceCollection services)
        {
            services.AddSingleton<IEventRepository, AdoNetMultiTenantEventRepository>();
            services.AddSingleton<ISnapshotRepository, AdoNetMultitenantSnapshotRepository>();
            services.AddSingleton<AdoNet.Internal.Scripts, Scripts>();

            return services;
        }
    }
}
