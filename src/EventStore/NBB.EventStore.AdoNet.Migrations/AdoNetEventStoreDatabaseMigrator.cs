using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NBB.EventStore.AdoNet.Internal;

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

        public async Task ReCreateDatabaseObjects(string[] args)
        {
            try
            {
                await DropDatabaseObjects();
            }
            catch
            {
                // ignored
            }

            await CreateDatabaseObjects();
        }

        private async Task CreateDatabaseObjects()
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.CreateDatabaseObjects, cnx);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DropDatabaseObjects()
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.DropDatabaseObjects, cnx);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
