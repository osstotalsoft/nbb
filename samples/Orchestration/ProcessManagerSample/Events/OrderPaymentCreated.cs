using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderPaymentCreated(
        Guid OrderId,
        decimal Amount,
        int DocumentId,
        int SiteId
    ) : INotification;
}
