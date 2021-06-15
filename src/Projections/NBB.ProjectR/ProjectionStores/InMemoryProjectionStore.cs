using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProjectR.ProjectionStores
{
    class InMemoryProjectionStore<TProjection, TIdentity> : IProjectionStore<TProjection, TIdentity>
    {
        public Task<TProjection> LoadById(TIdentity id, CancellationToken cancellationToken)
        {
            return Task.FromResult(default(TProjection));
        }

        public Task Save(TProjection projection, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
