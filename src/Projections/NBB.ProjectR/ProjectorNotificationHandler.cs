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
        private readonly IProjector<TProjection> _innerProjector;
        private readonly IProjectionStore<TProjection> _projectionStore;
        private readonly IInterpreter _effectInterpreter;

        public ProjectorNotificationHandler(IProjector<TProjection> innerProjector, IProjectionStore<TProjection> projectionStore, IInterpreter effectInterpreter)
        {
            _innerProjector = innerProjector;
            _projectionStore = projectionStore;
            _effectInterpreter = effectInterpreter;
        }


        public async Task Handle(TEvent ev, CancellationToken cancellationToken)
        {
            var id = _innerProjector.Correlate(ev);
            if (id == null)
                return;
            
            var (projection, loadedAtVersion) = await _projectionStore.Load(id, cancellationToken);
            var (_, effect) = _innerProjector.Project(ev, projection);
            await _projectionStore.SaveEvent(ev, id, loadedAtVersion, cancellationToken);
            await _effectInterpreter.Interpret(effect, cancellationToken);
        }
    }
}
