using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Invoices.Domain.InvoiceAggregate.DomainEvents
{
    public class InvoiceCreated : DomainEvent
    {
        public Guid InvoiceId { get; }

        public decimal Amount { get; }

        public Guid ClientId { get; }

        public Guid? ContractId { get; }


        [JsonConstructor]
        private InvoiceCreated(Guid eventId, DomainEventMetadata metadata, Guid invoiceId, decimal amount, Guid clientId, Guid? contractId) 
            : base(eventId, metadata)
        {
            InvoiceId = invoiceId;
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }

        public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId)
            : this(Guid.NewGuid(), new DomainEventMetadata {CreationDate = DateTime.UtcNow, SequenceNumber = 0 }, 
            invoiceId, amount, clientId, contractId)
        {
        }

    }
}
