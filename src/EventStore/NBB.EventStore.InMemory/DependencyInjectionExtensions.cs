using Microsoft.Extensions.DependencyInjection;

namespace NBB.EventStore.InMemory
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection WithInMemoryEventRepository(this IServiceCollection services)
        {
            services.AddSingleton<IEventRepository, InMemoryRepository>();
            services.AddSingleton<ISnapshotRepository, InMemorySnapshotRepository>();

            return services;
        }
    }
}
