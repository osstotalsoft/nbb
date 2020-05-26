namespace NBB.SQLStreamStore.Migrations
{

    class Program
    {
        protected Program()
        {

        }

        static void Main()
        {
            var migrator = new SqlStreamStoreMigrator();
            migrator.MigrateDatabaseToLatestVersion().Wait();
        }
    }
}