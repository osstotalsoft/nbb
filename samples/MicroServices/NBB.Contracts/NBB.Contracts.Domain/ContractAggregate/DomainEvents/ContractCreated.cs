using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractCreated : DomainEvent
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }

        [JsonConstructor]
        private ContractCreated(Guid eventId, DomainEventMetadata metadata, Guid contractId, Guid clientId) 
            : base(eventId, metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
        }

        public ContractCreated(Guid contractId, Guid clientId)
            : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow }, contractId, clientId)
        {
        }
    }
}
