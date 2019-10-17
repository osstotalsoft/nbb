using System;
using NBB.Application.DataContracts;

namespace NBB.ProcessManager.Tests.Events
{
    public class OrderShipped : Event
    {
        public Guid OrderId { get; }
        public DateTime ShippingDate { get; }

        public OrderShipped(Guid orderId, DateTime shippingDate, EventMetadata metadata = null) : base(metadata)
        {
            OrderId = orderId;
            ShippingDate = shippingDate;
        }
    }
}