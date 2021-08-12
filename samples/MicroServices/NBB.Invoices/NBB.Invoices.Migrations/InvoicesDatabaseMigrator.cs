// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NBB.Invoices.Migrations
{
    public class InvoicesDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new InvoicesDbContextFactory().CreateDbContext(args);
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        public async Task EnsureDatabaseDeleted(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new InvoicesDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}
