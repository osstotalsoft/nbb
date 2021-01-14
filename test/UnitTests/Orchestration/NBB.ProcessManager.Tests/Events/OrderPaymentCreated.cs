using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderPaymentCreated(
        decimal Amount,
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}
