using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NBB.Payments.Migrations
{
    public class PaymentsDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args)
        {
            var dbContext = new PaymentsDbContextFactory().CreateDbContext(args);
            await dbContext.Database.MigrateAsync();
        }

        public async Task EnsureDatabaseDeleted(string[] args)
        {
            var dbContext = new PaymentsDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync();
        }
    }
}
