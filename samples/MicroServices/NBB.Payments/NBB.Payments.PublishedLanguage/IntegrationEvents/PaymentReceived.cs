using NBB.Application.DataContracts;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Payments.PublishedLanguage.IntegrationEvents
{
    public class PaymentReceived : Event
    {
        public Guid PayableId { get; private set; }
        public Guid PaymentId { get; private set; }
        public Guid? InvoiceId { get; private set; }
        public DateTime PaymentDate { get; private set; }


        [JsonConstructor]
        private PaymentReceived(Guid eventId, ApplicationMetadata metadata,
            Guid payableId, Guid paymentId, Guid? invoiceId, DateTime paymentDate)
            : base(eventId, metadata)
        {
            PayableId = payableId;
            PaymentId = paymentId;
            InvoiceId = invoiceId;
            PaymentDate = paymentDate;
        }

        public PaymentReceived(Guid payableId, Guid paymentId, Guid? invoiceId, DateTime paymentDate)
            : this(Guid.NewGuid(), new ApplicationMetadata {CreationDate = DateTime.Now}, 
                payableId, paymentId, invoiceId, paymentDate)
        {
        }
    }
}