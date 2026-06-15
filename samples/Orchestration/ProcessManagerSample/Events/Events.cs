// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Mediator;

namespace ProcessManagerSample.Events
{
    public record OrderCompleted(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentCreated(Guid OrderId, decimal Amount, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentExpired (Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderPaymentReceived(Guid OrderId, int DocumentId, int SiteId) : INotification;

    public record OrderShipped(Guid OrderId, int DocumentId, int SiteId) : INotification;

    // Mediator's source generator does not support open-generic message types,
    // so the timer event is closed over Guid.
    public record TimerTicked(Guid Id) : INotification;

    public record LoopcycleCompleted(Guid Id) : INotification;

    public record LoopCompleted(Guid Id) : INotification;
}
