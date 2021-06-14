using NBB.Core.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBB.ProjectR.ProjectionStores
{
    class InMemoryProjectionStore<TProjection, TIdentity> : IProjectionStore<TProjection, TIdentity>
    {
        public Effect<TProjection> LoadById(TIdentity id)
        {
            return Effect.Pure(default(TProjection));
        }

        public Effect<Unit> Save(TProjection projection)
        {
            return Effect.Pure(Unit.Value);
        }
    }
}
