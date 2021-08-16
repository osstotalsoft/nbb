// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IProjectorMarker { }
    public interface IProjector<TModel, TMessage, TIdentity> : IProjectorMarker
    {
        (TModel Model, Effect<TMessage> Effect) Project(TMessage message, TModel model);
        (TIdentity Identity, TMessage Message) Subscribe(INotification @event);

    }

    public interface IProjectionStore<TModel, in TMessage, in TIdentity>
    {
        Task<(TModel Model, int Version)> Load(TIdentity id, CancellationToken cancellationToken);
        Task Save(TMessage message, TIdentity id, int expectedVersion, TModel model, CancellationToken cancellationToken);
    }

    public interface IReadModelStore<TModel>
    {
        Task<TModel> Load(object id, CancellationToken cancellationToken);
    }
}
