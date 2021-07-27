using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IProjector<TProjection>
    {
        (TProjection Projection, Effect<Unit> Effect) Project(object ev, TProjection projection);
        object Correlate(object ev);
    }


    public interface IProjectionStore<TProjection>
    {
        Task<(TProjection Projection, int Version)> Load(object id, CancellationToken cancellationToken);
        Task Save<TEvent>(TEvent ev, object id, int expectedVersion, TProjection projection, CancellationToken cancellationToken);
    }
}
