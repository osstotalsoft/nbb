// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;

namespace NBB.ProjectR
{
    class ProjectionStore<TModel, TMessage, TIdentity>: 
        IProjectionStore<TModel, TMessage, TIdentity>,
        IReadModelStore<TModel>
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IProjector<TModel, TMessage, TIdentity> _projector;
        private readonly ProjectorMetadataAccessor _metadataAccessor;

        public ProjectionStore(IEventStore eventStore, ISnapshotStore snapshotStore, IProjector<TModel, TMessage, TIdentity> projector, ProjectorMetadataAccessor metadataAccessor)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _projector = projector;
            _metadataAccessor = metadataAccessor;
        }
        
        public async Task<(TModel Model, int Version)> Load(TIdentity id, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            var snapshotEnvelope = await _snapshotStore.LoadSnapshotAsync(stream, cancellationToken);
            var events = await _eventStore.GetEventsFromStreamAsync(stream, snapshotEnvelope?.AggregateVersion + 1, cancellationToken);
            var projection = (TModel)snapshotEnvelope?.Snapshot;
            foreach (var @event in events.Cast<TMessage>())
            {
                (projection, _) = _projector.Project(@event, projection);
            }

            return (projection, events.Count + (snapshotEnvelope?.AggregateVersion ?? 0));
        }

        public async Task<TModel> Load(object id, CancellationToken cancellationToken)
        {
            var (model, _) = await Load((TIdentity) id, cancellationToken);
            return model;
        }

        public async Task Save(TMessage message, TIdentity id, int expectedVersion, TModel projection, CancellationToken cancellationToken)
        {
            var stream = GetStreamFrom(id);
            await _eventStore.AppendEventsToStreamAsync(stream, new object[] {message}, expectedVersion, cancellationToken);
            var snapshotFrequency = _metadataAccessor.GetMetadataFor<TModel>().SnapshotFrequency;
            var projectionVersion = expectedVersion + 1;
            if (projectionVersion % snapshotFrequency == 0)
            {
                await _snapshotStore.StoreSnapshotAsync(new SnapshotEnvelope(projection, 1, stream), cancellationToken);
            }
        }

        private string GetStreamFrom(object identity) =>
            $"PROJ::{typeof(TModel).GetLongPrettyName()}::{identity}";
    }
}
