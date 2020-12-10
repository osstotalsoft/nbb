using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.EventStore.Abstractions;
using NBB.SQLStreamStore.Internal;
using SqlStreamStore;

namespace NBB.SQLStreamStore
{
    public static class DependencyInjectionExtensions
    {
        public static void AddSqlStreamStore(this IServiceCollection services)
        {
            services.AddScoped<IEventStore, SqlStreamStore>();
            services.AddScoped<ISnapshotStore, NullSnapshotStore>();
            services.AddSingleton<ISerDes, SerDes>();
            services.AddTransient<IEventStoreSubscriber, SqlStreamStoreSubscriber>();


            services.AddSingleton(sp =>
            {
                var configuration = sp.GetService<IConfiguration>();
                var connectionString = configuration["EventStore:SqlStreamStore:ConnectionString"];

                return new MsSqlStreamStoreV3Settings(connectionString);
            });
            services.AddScoped<IStreamStore, MsSqlStreamStoreV3>();

        }
    }
}
