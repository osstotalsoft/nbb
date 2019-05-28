using NBB.Domain;
using System;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractCreated : DomainEvent
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }

        public ContractCreated(Guid contractId, Guid clientId, DomainEventMetadata metadata = null) 
            : base(metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
        }
    }
}
