using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;

namespace NBB.ProjectR
{
    class ProjectionStore<TProjection> : IProjectionStore<TProjection> where TProjection : class
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IProjector<TProjection> _projector;
        private readonly ProjectorMetadataAccessor _metadataAccessor;

        public ProjectionStore(IEventStore eventStore, ISnapshotStore snapshotStore, IProjector<TProjection> projector, ProjectorMetadataAccessor metadataAccessor)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _projector = projector;
            _metadataAccessor = metadataAccessor;
        }
        
        public async Task<(TProjection Projection, int Version)> Load(object id, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            var snapshotEnvelope = await _snapshotStore.LoadSnapshotAsync(stream, cancellationToken);
            var events = await _eventStore.GetEventsFromStreamAsync(stream, snapshotEnvelope?.AggregateVersion + 1, cancellationToken);
            var projection = snapshotEnvelope?.Snapshot as TProjection;
            foreach (var @event in events)
            {
                (projection, _) = _projector.Project(@event, projection);
            }

            return (projection, events.Count);
        }

        public async Task Save<TEvent>(TEvent ev, object id, int expectedVersion, TProjection projection, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            await _eventStore.AppendEventsToStreamAsync(stream, new object[] {ev}, expectedVersion, cancellationToken);
            var snapshotFrequency = _metadataAccessor.GetMetadataFor<TProjection>().SnapshotFrequency;
            var projectionVersion = expectedVersion + 1;
            if (projectionVersion % snapshotFrequency == 0)
            {
                await _snapshotStore.StoreSnapshotAsync(new SnapshotEnvelope(projection, default, stream), cancellationToken);
            }
        }

        private string GetStreamFrom(object identity) =>
            $"PROJ::{typeof(TProjection).GetLongPrettyName()}::{identity}";
    }
}
