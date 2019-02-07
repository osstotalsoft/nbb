namespace NBB.EventStore.Infrastructure
{
    public class EventStoreOptionsBuilder
    {
        public EventStoreOptions Options { get; }

        public EventStoreOptionsBuilder()
        {
            Options = new EventStoreOptions();
        }

    }
}
