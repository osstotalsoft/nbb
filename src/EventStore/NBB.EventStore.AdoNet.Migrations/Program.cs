// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.EventStore.AdoNet.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
        }
    }
}
