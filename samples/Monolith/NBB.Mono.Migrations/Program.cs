using NBB.Contracts.Migrations;
using NBB.EventStore.AdoNet.Migrations;
using NBB.Invoices.Migrations;
using NBB.Payments.Migrations;

namespace NBB.Mono.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            new ContractsReadDatabaseMigrator().MigrateDatabaseToLatestVersion(args).Wait();
            new InvoicesDatabaseMigrator().MigrateDatabaseToLatestVersion(args).Wait();
            new PaymentsDatabaseMigrator().MigrateDatabaseToLatestVersion(args).Wait();

            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
        }
    }
}