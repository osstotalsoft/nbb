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