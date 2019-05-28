using NBB.Domain;
using System;

namespace NBB.Payments.Domain.PayableAggregate.DomainEvents
{
    public class PayableCreated : DomainEvent
    {
        public Guid PayableId { get; }
        public Guid? InvoiceId { get; }
        public Guid ClientId { get; }
        public decimal Amount { get; }

        public PayableCreated(Guid payableId, Guid? invoiceId, Guid clientId, decimal amount, DomainEventMetadata metadata = null) 
            : base(metadata)
        {
            PayableId = payableId;
            InvoiceId = invoiceId;
            ClientId = clientId;
            Amount = amount;
        }
    }
}
