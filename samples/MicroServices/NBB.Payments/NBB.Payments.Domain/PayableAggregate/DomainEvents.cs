using System;
using MediatR;

namespace NBB.Payments.Domain.PayableAggregate
{
    public record PayableCreated(Guid PayableId, Guid? InvoiceId, Guid ClientId, decimal Amount) : INotification;

    public record PaymentReceived
        (Guid PaymentId, Guid PayableId, Guid? InvoiceId, DateTime PaymentDate) : INotification;
}