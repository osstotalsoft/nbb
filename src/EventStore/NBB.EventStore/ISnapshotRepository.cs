using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore
{
    public interface ISnapshotRepository
    {
        Task<SnapshotDescriptor> LoadSnapshotAsync(string stream, CancellationToken cancellationToken);
        Task StoreSnapshotAsync(string stream, SnapshotDescriptor snapshotDescriptor, CancellationToken cancellationToken);
    }
}