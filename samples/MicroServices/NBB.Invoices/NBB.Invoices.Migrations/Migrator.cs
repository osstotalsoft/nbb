// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Invoices.Data;
using NBB.EventStore.AdoNet.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Migrations
{
    public class Migrator(IConfiguration configuration, ILogger<Migrator> logger)
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var options = new DbContextOptionsBuilder<InvoicesDbContext>()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("NBB.Invoices.Migrations"))
                .Options;

            await using (var dbContext = new InvoicesDbContext(options))
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            logger.LogInformation("Database is up to date");

            await new AdoNetEventStoreDatabaseMigrator(configuration).CreateDatabaseObjectsAsync(cancellationToken);
            logger.LogInformation("EventStore objects are up to date");
        }
    }
}
