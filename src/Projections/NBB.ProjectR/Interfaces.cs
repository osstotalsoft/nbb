using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IProjector { }
    public interface IProjector<TModel, TMessage, TIdentity> : IProjector
    {
        (TModel Model, Effect<TMessage> Effect) Project(TMessage message, TModel model);
        (TIdentity Identity, TMessage Message) Subscribe(INotification @event);
        
    }

    public interface ISubscribeTo
    {
    }
    public interface ISubscribeTo<T1> : ISubscribeTo { }
    public interface ISubscribeTo<T1, T2> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3, T4> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3, T4, T5> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6, T7> : ISubscribeTo{ }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6, T7, T8> : ISubscribeTo{ }

    public interface IProjectionStore<TModel, in TMessage, in TIdentity>
    {
        Task<(TModel Model, int Version)> Load(TIdentity id, CancellationToken cancellationToken);
        Task Save(TMessage message, TIdentity id, int expectedVersion, TModel model, CancellationToken cancellationToken);
    }
}
