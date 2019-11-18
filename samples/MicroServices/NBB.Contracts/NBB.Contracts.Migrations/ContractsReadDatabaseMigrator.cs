using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Migrations
{
    public class ContractsReadDatabaseMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new ContractsReadDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        public async Task EnsureDatabaseDeleted(string[] args, CancellationToken cancellationToken = default)
        {
            var dbContext = new ContractsReadDbContextFactory().CreateDbContext(args);
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}
