// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.InMemory
{
    public class InMemorySnapshotRepository : ISnapshotRepository
    {
        private readonly ConcurrentDictionary<string, SnapshotDescriptor> _storage =
            new();

        public Task<SnapshotDescriptor> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default)
        {
            var snapshotDescriptor = _storage.GetValueOrDefault(stream, null);
            return Task.FromResult(snapshotDescriptor);
        }

        public Task StoreSnapshotAsync(string stream, SnapshotDescriptor snapshotDescriptor, CancellationToken cancellationToken = default)
        {
            _storage.AddOrUpdate(stream,
                key => snapshotDescriptor,
                (key, value) => snapshotDescriptor);

            return Task.CompletedTask;
        }
    }
}
