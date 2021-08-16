// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.EventStore.Abstractions;
using NBB.SQLStreamStore;
using NBB.SQLStreamStore.Internal;
using SqlStreamStore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddSqlStreamStore(this IServiceCollection services)
        {
            services.AddScoped<IEventStore, NBB.SQLStreamStore.SqlStreamStore>();
            services.AddScoped<ISnapshotStore, NullSnapshotStore>();
            services.AddSingleton<ISerDes, SerDes>();

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
