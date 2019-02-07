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
