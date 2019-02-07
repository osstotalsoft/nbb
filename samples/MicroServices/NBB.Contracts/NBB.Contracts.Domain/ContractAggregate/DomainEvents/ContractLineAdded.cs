using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractLineAdded : DomainEvent
    {
        public Guid ContractLineId { get; }

        public Guid ContractId { get; }

        public string Product { get; }

        public decimal Price { get; }

        public int Quantity { get; }

        [JsonConstructor]
        private ContractLineAdded(Guid eventId, DomainEventMetadata metadata,
            Guid contractLineId, Guid contractId, string product, decimal price, int quantity)
            : base(eventId, metadata)
        {
            ContractLineId = contractLineId;
            ContractId = contractId;
            Product = product;
            Price = price;
            Quantity = quantity;
        }

        public ContractLineAdded(Guid contractLineId, Guid contractId, string product, decimal price, int quantity)
            : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow }, contractLineId, contractId, product, price, quantity)
        {
        }
    }
}