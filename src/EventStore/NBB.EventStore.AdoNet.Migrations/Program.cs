namespace NBB.EventStore.AdoNet.Migrations
{
    class Program
    {
        protected Program()
        {

        }

        static void Main(string[] args)
        {
            new AdoNetEventStoreDatabaseMigrator().ReCreateDatabaseObjects(args).Wait();
        }
    }
}
