using NBB.Application.DataContracts;
using System;

namespace ProcessManagerSample.Events
{
    public class OrderPaymentExpired : Event
    {
        public Guid OrderId { get; }
        public int DocumentId { get; }
        public int SiteId { get; }

        public OrderPaymentExpired(Guid orderId, int documentId, int siteId, EventMetadata metadata = null)
            : base(metadata)
        {
            OrderId = orderId;
            DocumentId = documentId;
            SiteId = siteId;
        }
    }
}
