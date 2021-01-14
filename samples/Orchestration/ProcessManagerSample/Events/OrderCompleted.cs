using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderCompleted(
        Guid OrderId,
        decimal Amount,
        int DocumentId,
        int SiteId
    ) : INotification;
}
