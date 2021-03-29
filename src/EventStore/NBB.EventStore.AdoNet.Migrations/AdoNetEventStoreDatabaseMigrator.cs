using Microsoft.Extensions.Configuration;
using NBB.EventStore.AdoNet.Internal;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.AdoNet.Migrations
{
    public class AdoNetEventStoreDatabaseMigrator
    {
        private readonly string _connectionString;
        private readonly Scripts _scripts;

        public AdoNetEventStoreDatabaseMigrator()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();
            _connectionString = configuration.GetSection("EventStore").GetSection("NBB")["ConnectionString"];
            _scripts = new Scripts();
        }

        public async Task ReCreateDatabaseObjects(string[] args, CancellationToken cancellationToken = default)
        {
            try
            {
                await DropDatabaseObjectsAsync(cancellationToken);
            }
            catch
            {
                // ignored
            }

            await CreateDatabaseObjectsAsync(cancellationToken);
        }

        public async Task CreateDatabaseObjectsAsync(CancellationToken cancellationToken = default)
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                await cnx.OpenAsync(cancellationToken);

                var cmd = new SqlCommand(_scripts.CreateDatabaseObjects, cnx);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task DropDatabaseObjectsAsync(CancellationToken cancellationToken = default)
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                await cnx.OpenAsync(cancellationToken);

                var cmd = new SqlCommand(_scripts.DropDatabaseObjects, cnx);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }
}
