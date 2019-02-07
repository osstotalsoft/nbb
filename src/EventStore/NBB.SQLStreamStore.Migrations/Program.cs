namespace NBB.SQLStreamStore.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            var migrator = new SqlStreamStoreMigrator();
            migrator.MigrateDatabaseToLatestVersion(args).Wait();

        }
    }
}
