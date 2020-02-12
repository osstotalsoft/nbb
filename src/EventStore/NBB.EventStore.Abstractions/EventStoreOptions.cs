namespace NBB.EventStore.Abstractions
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; }
        public string TopicSufix { get; set; }
    }
}