// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore.AdoNet.Migrations;
using System;

namespace NBB.Payments.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            var paymentsMigrator = new PaymentsDatabaseMigrator();
            paymentsMigrator.EnsureDatabaseDeleted(args).Wait();
            Console.WriteLine("Database deleted");
            paymentsMigrator.MigrateDatabaseToLatestVersion(args).Wait();
            Console.WriteLine("Database created");

            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
            Console.WriteLine("EventStore objects re-created");
        }
    }
}