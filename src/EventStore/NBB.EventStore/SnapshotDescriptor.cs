namespace NBB.EventStore
{
    public class SnapshotDescriptor
    {
        public string SnapshotType { get; }
        public string SnapshotData { get; }
        public string StreamId { get; }
        public int AggregateVersion { get; }

        public SnapshotDescriptor(string snapshotType, string snapshotData, string streamId, int aggregateVersion)
        {
            SnapshotType = snapshotType;
            SnapshotData = snapshotData;
            StreamId = streamId;
            AggregateVersion = aggregateVersion;
        }
    }
}
