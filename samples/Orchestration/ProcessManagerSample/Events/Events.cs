using System;
using MediatR;

namespace ProcessManagerSample.Events
{
    public record OrderCompleted(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentExpired (Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentReceived(Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderShipped(Guid OrderId, int DocumentId, int SiteId) : INotification;
}