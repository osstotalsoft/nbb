using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProjectR
{
    public interface IHaveIdentityOf<TIdentity>{}

    public interface IProject<in TEvent, TProjection>
    {
        TProjection Project(TEvent ev, TProjection projection);
    }

    public interface ICorrelate<TProjection, TIdentity>
    {
        Maybe<TIdentity> Correlate<TEvent>(TEvent ev);
    }

    public interface IProjectionStore<TProjection, in TIdentity>
    {
        Task<TProjection> LoadById(TIdentity id, CancellationToken cancellationToken);
        Task Save(TProjection projection, CancellationToken cancellationToken);
    }

    
}
