using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderCompleted(
        decimal Amount,
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}
