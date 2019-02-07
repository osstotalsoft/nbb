using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Payments.Domain.PayableAggregate.DomainEvents
{
    public class PayableCreated : DomainEvent
    {
        public Guid PayableId { get; }
        public Guid? InvoiceId { get; }

        public Guid ClientId { get; }
        public decimal Amount { get; }


        [JsonConstructor]
        private PayableCreated(Guid eventId, DomainEventMetadata metadata, Guid payableId, Guid? invoiceId, Guid clientId, decimal amount) 
            : base(eventId, metadata)
        {
            PayableId = payableId;
            InvoiceId = invoiceId;
            ClientId = clientId;
            Amount = amount;
        }

        public PayableCreated(Guid payableId, Guid? invoiceId, Guid clientId, decimal amount)
            : this(Guid.NewGuid(), new DomainEventMetadata{ CreationDate = DateTime.UtcNow }, payableId, invoiceId, clientId, amount)
        {
        }
    }
}
