namespace NBB.EventStore.AdoNet.Multitenancy.Internal
{
    public class Scripts : AdoNet.Internal.Scripts
    {
        public Scripts()
            : base(typeof(Scripts).Assembly, "NBB.EventStore.AdoNet.MultiTenancy.Internal.SqlScripts")
        {
        }
    }
}
