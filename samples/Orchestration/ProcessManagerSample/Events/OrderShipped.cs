using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderShipped(
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}