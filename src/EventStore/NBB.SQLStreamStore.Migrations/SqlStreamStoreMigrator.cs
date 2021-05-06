using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SqlStreamStore;

namespace NBB.SQLStreamStore.Migrations
{
    public class SqlStreamStoreMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(CancellationToken cancellationToken = default)
        {
            var store = GetStore();
            await store.CreateSchemaIfNotExists(cancellationToken);
        }

        public async Task EnsureDatabaseDeleted(CancellationToken cancellationToken = default)
        {
            var store = GetStore();
            try
            {
                await store.DropAll(cancellationToken);
            }
            catch
            {
                // will fail if the database does not exist
            }
        }

        private MsSqlStreamStoreV3 GetStore()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetEntryAssembly());
            }

            var configuration = configurationBuilder.Build();
            var connectionString = configuration["EventStore:SqlStreamStore:ConnectionString"];
            var settings = new MsSqlStreamStoreV3Settings(connectionString);
            var store = new MsSqlStreamStoreV3(settings);

            return store;
        }
    }
}
