using System.Threading.Tasks;

namespace NBB.Contracts.Migrations
{
    public class ContractsReadDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args)
        {
            var dbContext = new ContractsReadDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task EnsureDatabaseDeleted(string[] args)
        {
            var dbContext = new ContractsReadDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync();
        }
    }
}
