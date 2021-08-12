// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Payments.Migrations
{
    public class PaymentsDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new PaymentsDbContextFactory().CreateDbContext(args);
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        public async Task EnsureDatabaseDeleted(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new PaymentsDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}