using NBB.Core.Effects;
using System;

namespace ProjectR
{
    record ContractCreated(Guid ContractId, decimal Value);
    record ContractValidated(Guid ContractId);

    record ContractProjection(Guid ContractId, bool IsValidated) : IHaveIdentityOf<Guid>;

    class ContractProjector :
        IProject<ContractCreated, ContractProjection>,
        IProject<ContractValidated, ContractProjection>
    {
        public Effect<ContractProjection> Project(ContractCreated ev, ContractProjection projection)
            => Effect.Pure(new ContractProjection(ev.ContractId, false));

        public Effect<ContractProjection> Project(ContractValidated ev, ContractProjection projection)
            => Effect.Pure(projection with { IsValidated = true });
    }

    class ContractCorrelation
    : ICorrelate<ContractProjection, Guid>
    {
        public Maybe<Guid> Correlate<TEvent>(TEvent ev) => ev switch
        {
            ContractCreated { Value: >= 0 } cc => cc.ContractId,
            ContractValidated cv => cv.ContractId,
            _ => Maybe<Guid>.Nothing
        };
    }
}
