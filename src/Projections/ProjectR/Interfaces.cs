using NBB.Core.Effects;

namespace ProjectR
{

    public interface IProjector
    {
        Effect<Unit> Project(object ev);
    }
    public interface IEventProjector<in TEvent>
    {
        Effect<Unit> Project(TEvent ev);
    }
    
    public interface IIdentityProvider<out TIdentity>
    {
        TIdentity Identity { get; }
    }
    
    public interface IHaveIdentityOf<TIdentity>{}

    public interface IProject<in TEvent, TProjection>
    {
        Effect<TProjection> Project(TEvent ev, TProjection projection);
    }

    public interface ICorrelate<TProjection, TIdentity>
    {
        Maybe<TIdentity> Correlate<TEvent>(TEvent ev);
    }

    public interface IProjectionStore<TProjection, in TIdentity>
    {
        Effect<TProjection> LoadById(TIdentity id);
        Effect<Unit> Save(TProjection projection);
    }

    
}
