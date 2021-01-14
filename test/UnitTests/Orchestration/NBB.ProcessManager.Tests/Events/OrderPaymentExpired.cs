using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderPaymentExpired (
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}
