using NBB.Domain;
using System;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractValidated : DomainEvent
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }
        public decimal Amount { get; }

        public ContractValidated(Guid contractId, Guid clientId, decimal amount, DomainEventMetadata metadata = null)
            : base(metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
            Amount = amount;
        }
    }
}