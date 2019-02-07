using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractValidated : DomainEvent
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }
        public decimal Amount { get; }

        [JsonConstructor]
        private ContractValidated(Guid eventId, DomainEventMetadata metadata,
            Guid contractId, Guid clientId, decimal amount)
            : base(eventId, metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
            Amount = amount;
        }

        public ContractValidated(Guid contractId, Guid clientId, decimal amount)
            : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow }, contractId, clientId, amount)
        {
        }

    }
}