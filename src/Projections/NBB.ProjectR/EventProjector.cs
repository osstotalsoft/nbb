using NBB.Core.Effects;

namespace NBB.ProjectR
{

    class EventProjector<TEvent, TProjection, TIdentity> : IEventProjector<TEvent>
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

        public Effect<Unit> Project(TEvent ev)
        {
            var id = _correlate.Correlate(ev);
            return id.HasValue
                ? _projectionStore.LoadById(id.Value)
                    .Then(proj => _innerProject.Project(ev, proj))
                    .Then(_projectionStore.Save)
                : Effect.Pure(Unit.Value);
        }
    }
}
