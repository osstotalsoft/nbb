using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IProjector { }
    public interface IProjector<TModel> : IProjector
    {
        (TModel Model, Effect<IMessage<TModel>> Effect) Project(IMessage<TModel> message, TModel model);
        (object Identity, IMessage<TModel>) Subscribe(object @event);
    }

    public interface IProjector<TModel, T1> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3, T4> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3, T4, T5> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3, T4, T5, T6> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3, T4, T5, T6, T7> : IProjector<TModel> { }
    public interface IProjector<TModel, T1, T2, T3, T4, T5, T6, T7, T8> : IProjector<TModel> { }

    public interface IMessage<TModel>
    {
    }

    public interface IEvent<TModel>
    {

    }

    public interface IProjectionStore<TModel>
    {
        Task<(TModel Projection, int Version)> Load(object id, CancellationToken cancellationToken);
        Task Save(IMessage<TModel> message, object id, int expectedVersion, TModel projection, CancellationToken cancellationToken);
    }
}
