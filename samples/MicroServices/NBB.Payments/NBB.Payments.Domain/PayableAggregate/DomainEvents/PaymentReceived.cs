using NBB.Domain;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Payments.Domain.PayableAggregate.DomainEvents
{
    public class PaymentReceived : DomainEvent
    {
        public Guid PaymentId { get; private set; }
        public Guid PayableId { get; private set; }

        public Guid? InvoiceId { get; private set; }
        public DateTime PaymentDate { get; private set; }


        [JsonConstructor]
        private PaymentReceived(Guid eventId, DomainEventMetadata metadata, Guid paymentId, Guid payableId, Guid? invoiceId, DateTime paymentDate)
            : base(eventId, metadata)
        {
            PaymentId = paymentId;
            PayableId = payableId;
            InvoiceId = invoiceId;
            PaymentDate = paymentDate;
        }

        public PaymentReceived(Guid paymentId, Guid payableId, Guid? invoiceId, DateTime paymentDate)
            : this(Guid.NewGuid(),
                new DomainEventMetadata { CreationDate = DateTime.UtcNow, SequenceNumber = 0 },
                paymentId, payableId, invoiceId, paymentDate)
        {
        }

    }
}
