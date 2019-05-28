using NBB.Application.DataContracts;
using System;

namespace NBB.Payments.PublishedLanguage.IntegrationEvents
{
    public class PaymentReceived : Event
    {
        public Guid PayableId { get; }
        public Guid PaymentId { get; }
        public Guid? InvoiceId { get; }
        public DateTime PaymentDate { get; }


        public PaymentReceived(Guid payableId, Guid paymentId, Guid? invoiceId, DateTime paymentDate, EventMetadata metadata = null)
            : base(metadata)
        {
            PayableId = payableId;
            PaymentId = paymentId;
            InvoiceId = invoiceId;
            PaymentDate = paymentDate;
        }
    }
}