// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    /// <summary>
    /// Applies an event to a projection: subscribes the projector to the event, then projects, saves and interprets effects
    /// until no more messages are produced. Invoked by the mediator adapters (NBB.ProjectR.MediatR, NBB.ProjectR.Mediator).
    /// </summary>
    public class ProjectorEventProcessor<TEvent, TModel, TMessage, TIdentity>
    {
        private readonly IProjector<TModel, TMessage, TIdentity> _projector;
        private readonly IProjectionStore<TModel, TMessage, TIdentity> _projectionStore;
        private readonly IInterpreter _effectInterpreter;

        public ProjectorEventProcessor(IProjector<TModel, TMessage, TIdentity> projector, IInterpreter effectInterpreter, IProjectionStore<TModel, TMessage, TIdentity> projectionStore)
        {
            _projector = projector;
            _effectInterpreter = effectInterpreter;
            _projectionStore = projectionStore;
        }

        public async Task Handle(TEvent ev, CancellationToken cancellationToken)
        {
            var (projectionId, message)  = _projector.Subscribe(ev);
            while (message is not null)
            {
                var (projection, loadedAtVersion) = await _projectionStore.Load(projectionId, cancellationToken);
                var (newProjection, effect) = _projector.Project(message, projection);
                await _projectionStore.Save(message, projectionId, loadedAtVersion, newProjection, cancellationToken);
                message = await _effectInterpreter.Interpret(effect, cancellationToken);
            }
        }
    }
}
