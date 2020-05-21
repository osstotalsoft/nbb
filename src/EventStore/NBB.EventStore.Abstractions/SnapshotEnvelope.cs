namespace NBB.EventStore.Abstractions
{
    public class SnapshotEnvelope
    {
        public object Snapshot { get; }
        public int AggregateVersion { get; }
        public string StreamId { get; }

        public SnapshotEnvelope(object snapshot, int aggregateVersion, string streamId)
        {
            Snapshot = snapshot;
            AggregateVersion = aggregateVersion;
            StreamId = streamId;
        }
    }
}
