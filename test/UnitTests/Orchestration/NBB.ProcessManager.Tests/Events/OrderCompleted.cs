using System;
using NBB.Application.DataContracts;

namespace NBB.ProcessManager.Tests.Events
{
    public class OrderCompleted : Event
    {
        public decimal Amount { get; }
        public Guid OrderId { get; }
        public int DocumentId { get; }
        public int SiteId { get; }

        public OrderCompleted(decimal amount, Guid orderId, int documentId, int siteId, EventMetadata metadata = null)
            : base(metadata)
        {
            Amount = amount;
            OrderId = orderId;
            DocumentId = documentId;
            SiteId = siteId;
        }
    }
}
