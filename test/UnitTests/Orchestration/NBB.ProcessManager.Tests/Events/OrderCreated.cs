using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderCreated(
        Guid OrderId,
        decimal Amount,
        int DocumentId,
        int SiteId
    ) : INotification;
}