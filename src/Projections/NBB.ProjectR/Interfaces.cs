using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IHaveIdentityOf<TIdentity>{}

    public interface IProjector<in TEvent, TProjection, TIdentity>
    {
        (TProjection Projection, Effect<Unit> Effect) Project(TEvent ev, TProjection projection);
        Maybe<TIdentity> Correlate(TEvent ev);
    }
    
    public interface IProjectionStore<TProjection, in TIdentity>
    {
        Task<TProjection> LoadById(TIdentity id, CancellationToken cancellationToken);
        Task Save(TProjection projection, CancellationToken cancellationToken);
    }

    
}
