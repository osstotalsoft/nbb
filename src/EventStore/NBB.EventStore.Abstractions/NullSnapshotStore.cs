using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public class NullSnapshotStore : ISnapshotStore
    {
        public Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SnapshotEnvelope>(null);
        }

        public Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
