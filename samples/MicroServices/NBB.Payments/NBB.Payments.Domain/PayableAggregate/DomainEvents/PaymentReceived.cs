using NBB.Domain;
using System;

namespace NBB.Payments.Domain.PayableAggregate.DomainEvents
{
    public class PaymentReceived : DomainEvent
    {
        public Guid PaymentId { get; }
        public Guid PayableId { get; }
        public Guid? InvoiceId { get; }
        public DateTime PaymentDate { get; }

        public PaymentReceived(Guid paymentId, Guid payableId, Guid? invoiceId, DateTime paymentDate, DomainEventMetadata metadata = null)
            : base(metadata)
        {
            PaymentId = paymentId;
            PayableId = payableId;
            InvoiceId = invoiceId;
            PaymentDate = paymentDate;
        }

    }
}
