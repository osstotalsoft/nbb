// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Internal;

namespace NBB.EventStore
{
    public class SnapshotStore : ISnapshotStore
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IEventStoreSerDes _eventStoreSerDes;
        private readonly ILogger<SnapshotStore> _logger;

        public SnapshotStore(ISnapshotRepository snapshotRepository, IEventStoreSerDes eventStoreSerDes, ILogger<SnapshotStore> logger)
        {
            _snapshotRepository = snapshotRepository;
            _eventStoreSerDes = eventStoreSerDes;
            _logger = logger;
        }

        public async Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var snapshotDescriptor = await _snapshotRepository.LoadSnapshotAsync(stream, cancellationToken);
            if (snapshotDescriptor == null)
                return null;

            var snapshot = DeserializeSnapshot(stream, snapshotDescriptor);

            stopWatch.Stop();
            _logger.LogDebug("SnapshotStore.LoadSnapshotAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);

            return new SnapshotEnvelope(snapshot, snapshotDescriptor.AggregateVersion, stream);
        }

        public async Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var snapshotDescriptor = SerializeSnapshot(snapshotEnvelope.StreamId, snapshotEnvelope.Snapshot, snapshotEnvelope.AggregateVersion);
            await _snapshotRepository.StoreSnapshotAsync(snapshotEnvelope.StreamId, snapshotDescriptor, cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("SnapshotStore.StoreSnapshotAsync for {Stream} took {ElapsedMilliseconds} ms", snapshotEnvelope.StreamId, stopWatch.ElapsedMilliseconds);

        }

        private SnapshotDescriptor SerializeSnapshot(string streamId, object snapshot, int version)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
     
            var snapshotDescriptor = new SnapshotDescriptor(GetFullTypeName(snapshot.GetType()), _eventStoreSerDes.Serialize(snapshot), streamId, version);

            stopWatch.Stop();
            _logger.LogDebug("SnapshotStore.SerializeSnapshot for {Stream} took {ElapsedMilliseconds} ms", streamId, stopWatch.ElapsedMilliseconds);

            return snapshotDescriptor;
        }

        private object DeserializeSnapshot(string streamId, SnapshotDescriptor snapshotDescriptor)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var snapshot = _eventStoreSerDes.Deserialize(snapshotDescriptor.SnapshotData,
                Type.GetType(snapshotDescriptor.SnapshotType));

            stopWatch.Stop();
            _logger.LogDebug("SnapshotStore.DeserializeSnapshot for {Stream} took {ElapsedMilliseconds} ms", streamId, stopWatch.ElapsedMilliseconds);

            return snapshot;
        }

        private static string GetFullTypeName(Type type)
        {
            var result = type.FullName + ", " + type.Assembly.GetName().Name;
            return result;
        }

    }
}
