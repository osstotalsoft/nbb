using System;
using NBB.Application.DataContracts;

namespace ProcessManagerSample.Events
{
    public class OrderCompleted : Event
    {
        public Guid OrderId { get; }
        public decimal Amount { get; }
        public int DocumentId { get; }
        public int SiteId { get; }

        public OrderCompleted(Guid orderId, decimal amount, int documentId, int siteId, EventMetadata metadata = null) : base(metadata)
        {
            OrderId = orderId;
            Amount = amount;
            DocumentId = documentId;
            SiteId = siteId;
        }
    }
}
