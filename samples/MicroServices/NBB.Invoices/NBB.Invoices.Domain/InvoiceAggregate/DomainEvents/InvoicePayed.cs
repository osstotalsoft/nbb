using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Invoices.Domain.InvoiceAggregate.DomainEvents
{
    public class InvoicePayed : DomainEvent
    {
        public Guid InvoiceId { get; }
        public Guid PaymentId { get; }

        [JsonConstructor]
        private InvoicePayed(Guid eventId, DomainEventMetadata metadata,
            Guid invoiceId, Guid paymentId)
            : base(eventId, metadata)
        {
            InvoiceId = invoiceId;
            PaymentId = paymentId;
        }

        public InvoicePayed(Guid invoiceId, Guid paymentId)
            : this(Guid.NewGuid(), new DomainEventMetadata { CreationDate = DateTime.UtcNow, SequenceNumber = 0 },
                invoiceId, paymentId)
        {
        }
    }
}