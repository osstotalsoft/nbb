using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.ProjectR
{

    class ProjectorNotificationHandler<TEvent, TProjection> : INotificationHandler<TEvent>
        where TProjection : IEquatable<TProjection>
        where TEvent : INotification
    {
        private readonly IProjector<TProjection> _projector;
        private readonly IProjectionStore<TProjection> _projectionStore;
        private readonly IInterpreter _effectInterpreter;

        public ProjectorNotificationHandler(IProjector<TProjection> projector, IInterpreter effectInterpreter, IProjectionStore<TProjection> projectionStore)
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
