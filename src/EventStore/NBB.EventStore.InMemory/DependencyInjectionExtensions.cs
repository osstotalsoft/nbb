using NBB.EventStore;
using NBB.EventStore.InMemory;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
