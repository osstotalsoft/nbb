// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.SQLStreamStore.Migrations
{
    class Program
    {
        static void Main()
        {
            var migrator = new SqlStreamStoreMigrator();
            migrator.MigrateDatabaseToLatestVersion().Wait();
        }
    }
}