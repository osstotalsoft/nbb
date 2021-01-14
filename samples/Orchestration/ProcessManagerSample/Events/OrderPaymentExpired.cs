using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderPaymentExpired (
        Guid OrderId,
        int DocumentId,
        int SiteId
    ) : INotification;
}
