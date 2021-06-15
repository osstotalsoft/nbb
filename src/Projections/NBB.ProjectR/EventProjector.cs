using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProjectR
{

    class EventProjector<TEvent, TProjection, TIdentity> : INotificationHandler<TEvent>
        where TProjection : IEquatable<TProjection>
        where TEvent : INotification
    {
        private readonly IProject<TEvent, TProjection> _innerProject;
        private readonly ICorrelate<TProjection, TIdentity> _correlate;
        private readonly IProjectionStore<TProjection, TIdentity> _projectionStore;

        public EventProjector(IProject<TEvent, TProjection> innerProject, ICorrelate<TProjection, TIdentity> correlate, IProjectionStore<TProjection, TIdentity> projectionStore)
        {
            _innerProject = innerProject;
            _correlate = correlate;
            _projectionStore = projectionStore;

        }


        public async Task Handle(TEvent ev, CancellationToken cancellationToken)
        {
            var id = _correlate.Correlate(ev);
            if (!id.HasValue)
                return;
            var projection = await _projectionStore.LoadById(id.Value, cancellationToken);
            var newProjection = _innerProject.Project(ev, projection);
            if (!projection.Equals(newProjection))
                await _projectionStore.Save(newProjection, cancellationToken);
        }
    }
}
