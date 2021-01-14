using System;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderShipped(
        Guid OrderId,
        DateTime ShippingDate
    );
}