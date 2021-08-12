// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using NBB.MultiTenancy.Abstractions.Options;
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
        private readonly Internal.Scripts _scripts;

        public AdoNetEventStoreDatabaseMigrator(bool forceMultiTenant = false, bool isTestHost = false)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(isTestHost ? Assembly.GetCallingAssembly() : Assembly.GetEntryAssembly());
            }

            var configuration = configurationBuilder.Build();
            _connectionString = configuration.GetSection("EventStore").GetSection("NBB")["ConnectionString"];
            var tenancySection = configuration.GetSection("MultiTenancy");
            var tenancyOptions = tenancySection.Get<TenancyHostingOptions>();
            if ((tenancyOptions == null) && !forceMultiTenant)
            {
                _scripts = new Internal.Scripts();
            }
            else
            {
                _scripts = new Multitenancy.Internal.Scripts();
            }            
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
            await using var cnx = new SqlConnection(_connectionString);
            await cnx.OpenAsync(cancellationToken);

            var cmd = new SqlCommand(_scripts.CreateDatabaseObjects, cnx);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task DropDatabaseObjectsAsync(CancellationToken cancellationToken = default)
        {
            await using var cnx = new SqlConnection(_connectionString);
            await cnx.OpenAsync(cancellationToken);

            var cmd = new SqlCommand(_scripts.DropDatabaseObjects, cnx);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
