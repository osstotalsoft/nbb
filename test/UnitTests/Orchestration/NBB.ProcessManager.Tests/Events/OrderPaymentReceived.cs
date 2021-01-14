using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderPaymentReceived(
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}