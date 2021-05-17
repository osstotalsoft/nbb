using NBB.Core.Abstractions;
using NBB.Core.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProjectR
{
    record ContractCreated(Guid ContractId, Decimal Value);
    record ContractValidated(Guid ContractId);

    record ContractProjection(Guid ContractId, bool IsValidated);

    class ContractProjector :
        IProjector<ContractCreated, ContractProjection>,
        IProjector<ContractValidated, ContractProjection>
    {
        public Effect<ContractProjection> Project(ContractCreated ev, ContractProjection projection)
            => Effect.Pure(new ContractProjection(ev.ContractId, false));

        public Effect<ContractProjection> Project(ContractValidated ev, ContractProjection projection)
            => Effect.Pure(projection with{IsValidated = true});
    }

    class ContractProjectionLocator
    : IProjectionCorrelator<ContractProjection, Guid>
    {
        public Maybe<Guid> Correlate<TEvent>(TEvent ev) => ev switch
        {
            ContractCreated { Value: >= 0 } cc => cc.ContractId,
            ContractValidated cv => cv.ContractId,
            _ => Maybe<Guid>.Nothing
        };
    }
}
