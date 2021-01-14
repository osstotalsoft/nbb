using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderPaymentReceived(
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}