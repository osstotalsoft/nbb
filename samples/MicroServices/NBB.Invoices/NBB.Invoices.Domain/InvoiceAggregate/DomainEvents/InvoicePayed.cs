using NBB.Domain;
using System;

namespace NBB.Invoices.Domain.InvoiceAggregate.DomainEvents
{
    public class InvoicePayed : DomainEvent
    {
        public Guid InvoiceId { get; }
        public Guid PaymentId { get; }

        public InvoicePayed(Guid invoiceId, Guid paymentId, DomainEventMetadata metadata = null)
            : base(metadata)
        {
            InvoiceId = invoiceId;
            PaymentId = paymentId;
        }
    }
}