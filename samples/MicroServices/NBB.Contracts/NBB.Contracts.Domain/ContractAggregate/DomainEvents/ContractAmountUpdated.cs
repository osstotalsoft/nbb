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


        public ContractAmountUpdated(Guid contractId, decimal newAmount, DomainEventMetadata metadata = null)
            : base(metadata)
        {
            ContractId = contractId;
            NewAmount = newAmount;
        }
    }
}