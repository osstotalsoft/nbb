namespace NBB.Data.EventSourcing.Infrastructure
{
    public class EventSourcingOptionsBuilder
    {
        public EventSourcingOptions Options { get; }

        public EventSourcingOptionsBuilder()
        {
            Options = new EventSourcingOptions();
        }
    }
}
