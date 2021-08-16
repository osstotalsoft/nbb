// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.EventStore.AdoNet.Migrations;
using System;

namespace NBB.Contracts.Migrations
{
    class Program
    {
        private static void Main(string[] args)
        {
            var contractsMigrator = new ContractsReadDatabaseMigrator();
            contractsMigrator.EnsureDatabaseDeleted(args).Wait();
            Console.WriteLine("Database deleted");
            contractsMigrator.MigrateDatabaseToLatestVersion(args).Wait();
            Console.WriteLine("Database created");

            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
            Console.WriteLine("EventStore objects re-created");
        }
    }
}