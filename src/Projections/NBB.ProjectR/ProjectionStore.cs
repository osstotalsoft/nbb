using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;

namespace NBB.ProjectR
{
    class ProjectionStore<TProjection> : IProjectionStore<TProjection>
    {
        private readonly IEventStore _eventStore;
        private readonly IProjector<TProjection> _projector;

        public ProjectionStore(IEventStore eventStore, IProjector<TProjection> projector)
        {
            _eventStore = eventStore;
            _projector = projector;
        }
        
        public async Task<(TProjection Projection, int Version)> Load(object id, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            var events = await _eventStore.GetEventsFromStreamAsync(stream, null, cancellationToken);
            var projection = default(TProjection);
            foreach (var @event in events)
            {
                (projection, _) = _projector.Project(@event, projection);
            }

            return (projection, events.Count);
        }

        public async Task SaveEvent<TEvent>(TEvent ev, object id, int expectedVersion, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            await _eventStore.AppendEventsToStreamAsync(stream, new object[] {ev}, expectedVersion, cancellationToken);
        }

        private string GetStreamFrom(object identity) =>
            $"PROJ::{typeof(TProjection).GetLongPrettyName()}::{identity}";
    }
}
