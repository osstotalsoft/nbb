using System;
using System.Data;
using NBB.Core.Abstractions;
using NBB.Core.Effects;

namespace ProjectR
{

    public interface IProjector<in TEvent, TProjection>
    {
        Effect<TProjection> Project(TEvent ev, TProjection projection);
    }

    public interface IProjector<in TEvent>
    {
        Effect<Unit> Project(TEvent ev);
    }

    public interface IProjectionCorrelator<TProjection, TIdentity>
    {
        Maybe<TIdentity> Correlate<TEvent>(TEvent ev);
    }

    public interface IProjectionStore<TProjection, in TIdentity>
    {
        Effect<TProjection> LoadById(TIdentity id);
        Effect<Unit> Save(TProjection projection);
    }

    
}
