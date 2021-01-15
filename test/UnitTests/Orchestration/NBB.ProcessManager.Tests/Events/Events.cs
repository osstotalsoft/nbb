using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Events
{
    public record OrderCompleted(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentExpired (Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentReceived(Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderShipped(Guid OrderId, DateTime ShippingDate);
}