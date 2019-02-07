using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.Abstractions
{
    public interface ISnapshotStore
    {
        Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default);
        Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default);
    }
}
