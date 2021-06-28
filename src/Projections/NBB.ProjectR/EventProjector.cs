using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.ProjectR
{

    class EventProjector<TEvent, TProjection, TIdentity> : INotificationHandler<TEvent>
        where TProjection : IEquatable<TProjection>
        where TEvent : INotification
    {
        private readonly IProjector<TEvent, TProjection, TIdentity> _innerProjector;
        private readonly IProjectionStore<TProjection, TIdentity> _projectionStore;
        private readonly IInterpreter _effectInterpreter;

        public EventProjector(IProjector<TEvent, TProjection, TIdentity> innerProjector, IProjectionStore<TProjection, TIdentity> projectionStore, IInterpreter effectInterpreter)
        {
            _innerProjector = innerProjector;
            _projectionStore = projectionStore;
            _effectInterpreter = effectInterpreter;
        }


        public async Task Handle(TEvent ev, CancellationToken cancellationToken)
        {
            var id = _innerProjector.Correlate(ev);
            if (!id.HasValue)
                return;
            
            var projection = await _projectionStore.LoadById(id.Value, cancellationToken);
            var (newProjection, effect) = _innerProjector.Project(ev, projection);
            if (projection != null && !newProjection.Equals(projection))
                await _projectionStore.Save(newProjection, cancellationToken);
            await _effectInterpreter.Interpret(effect, cancellationToken);
        }
    }
}
