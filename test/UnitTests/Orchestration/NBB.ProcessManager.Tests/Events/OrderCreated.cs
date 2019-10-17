using System;
using NBB.Application.DataContracts;

namespace NBB.ProcessManager.Tests.Events
{
    public class OrderCreated : Event
    {
        public Guid OrderId { get; }
        public decimal Amount { get;  }
        public int DocumentId { get; }
        public int SiteId { get; }

        public OrderCreated(Guid orderId, decimal amount, int documentId, int siteId, EventMetadata metadata = null) : base(metadata)
        {
            OrderId = orderId;
            Amount = amount;
            DocumentId = documentId;
            SiteId = siteId;
        }
    }
}