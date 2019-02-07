using System;
using NBB.EventStore.AdoNet.Migrations;

namespace NBB.Invoices.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            var invoicesMigrator = new InvoicesDatabaseMigrator();
            invoicesMigrator.EnsureDatabaseDeleted(args).Wait();
            Console.WriteLine("Database deleted");
            invoicesMigrator.MigrateDatabaseToLatestVersion(args).Wait();
            Console.WriteLine("Database created");


            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
            Console.WriteLine("EventStore objects re-created");
        }
    }
}
