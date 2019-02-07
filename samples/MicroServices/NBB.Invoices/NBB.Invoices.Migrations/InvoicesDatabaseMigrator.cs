using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NBB.Invoices.Migrations
{
    public class InvoicesDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args)
        {
            var dbContext = new InvoicesDbContextFactory().CreateDbContext(args);
            await dbContext.Database.MigrateAsync();
        }

        public async Task EnsureDatabaseDeleted(string[] args)
        {
            var dbContext = new InvoicesDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync();
        }
    }
}
