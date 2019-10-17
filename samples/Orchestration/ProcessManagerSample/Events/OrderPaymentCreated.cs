using NBB.Application.DataContracts;
using System;

namespace ProcessManagerSample.Events
{
    public class OrderPaymentCreated : Event
    {
        public decimal Amount { get; }
        public Guid OrderId { get; }
        public int DocumentId { get; }
        public int SiteId { get; }

        public OrderPaymentCreated(Guid orderId, decimal amount, int documentId, int siteId, EventMetadata metadata = null)
            : base(metadata)
        {
            Amount = amount;
            OrderId = orderId;
            DocumentId = documentId;
            SiteId = siteId;
        }
    }
}
