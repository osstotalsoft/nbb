namespace NBB.Data.EventSourcing.Infrastructure
{
    public class EventSourcingOptions
    {
        public int DefaultSnapshotVersionFrequency { get; set; } = 10;
    }
}
