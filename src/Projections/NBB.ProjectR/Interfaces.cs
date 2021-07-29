using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IProjector { }
    public interface IProjector<TProjection> : IProjector
    {
        (TProjection Projection, Effect<Unit> Effect) Project(IMessage<TProjection> message, TProjection projection);
        IMessage<TProjection> Subscribe(object @event);
        object Identify(IMessage<TProjection> message);
    }

    public interface IProjector<TProjection, T1> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3, T4> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3, T4, T5> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3, T4, T5, T6> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3, T4, T5, T6, T7> : IProjector<TProjection> { }
    public interface IProjector<TProjection, T1, T2, T3, T4, T5, T6, T7, T8> : IProjector<TProjection> { }

    public interface IMessage<TProjection>
    {
    }


    public interface IProjectionStore<TProjection>
    {
        Task<(TProjection Projection, int Version)> Load(object id, CancellationToken cancellationToken);
        Task Save(IMessage<TProjection> message, object id, int expectedVersion, TProjection projection, CancellationToken cancellationToken);
    }
}
