// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public interface ISnapshotStore
    {
        Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default);
        Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default);
    }
}
