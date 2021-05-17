using NBB.Core.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectR
{
    class Projector<TEvent, TProjection, TIdentity> : IProjector<TEvent>
    {
        private readonly IProjector<TEvent, TProjection> _innerProjector;
        private readonly IProjectionCorrelator<TProjection, TIdentity> _projectionCorrelator;
        private readonly IProjectionStore<TProjection, TIdentity> _projectionStore;

        public Projector(IProjector<TEvent, TProjection> innerProjector, IProjectionCorrelator<TProjection, TIdentity> projectionCorrelator, IProjectionStore<TProjection, TIdentity> projectionStore)
        {
            _innerProjector = innerProjector;
            _projectionCorrelator = projectionCorrelator;
            _projectionStore = projectionStore;

        }

        public Effect<Unit> Project(TEvent ev)
        {
            var id = _projectionCorrelator.Correlate(ev);
            return id.HasValue
                ? _projectionStore.LoadById(id.Value)
                    .Then(proj => _innerProjector.Project(ev, proj))
                    .Then(_projectionStore.Save)
                : Effect.Pure(Unit.Value);
        }
    }
}
