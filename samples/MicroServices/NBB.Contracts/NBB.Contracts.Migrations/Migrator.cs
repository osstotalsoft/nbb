// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Contracts.ReadModel.Data;
using NBB.EventStore.AdoNet.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Migrations
{
    public class Migrator(IConfiguration configuration, ILogger<Migrator> logger)
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var options = new DbContextOptionsBuilder<ContractsReadDbContext>()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("NBB.Contracts.Migrations"))
                .Options;

            await using (var dbContext = new ContractsReadDbContext(options))
            {
                var created = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation(created
                    ? "Read model database created"
                    : "Read model database already exists, schema left unchanged");
            }

            await new AdoNetEventStoreDatabaseMigrator(configuration).CreateDatabaseObjectsAsync(cancellationToken);
            logger.LogInformation("EventStore objects are up to date");
        }
    }
}
