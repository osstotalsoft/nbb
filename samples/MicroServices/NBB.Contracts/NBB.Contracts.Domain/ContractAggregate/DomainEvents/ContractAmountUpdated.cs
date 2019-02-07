using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractAmountUpdated : DomainEvent
    {
        public Guid ContractId { get; }
        public decimal NewAmount { get; }


        [JsonConstructor]
        private ContractAmountUpdated(Guid eventId, DomainEventMetadata metadata, Guid contractId, decimal newAmount)
            : base(eventId, metadata)
        {
            ContractId = contractId;
            NewAmount = newAmount;
        }

        public ContractAmountUpdated(Guid contractId, decimal newAmount)
            : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow }, contractId, newAmount)
        {
        }
    }
}