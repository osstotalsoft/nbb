using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderCreated(
        Guid OrderId,
        decimal Amount,
        int DocumentId,
        int SiteId
    ) : INotification;
}
