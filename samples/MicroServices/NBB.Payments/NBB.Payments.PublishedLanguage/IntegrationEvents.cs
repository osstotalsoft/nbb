using System;
using MediatR;

namespace NBB.Payments.PublishedLanguage
{
    public record PaymentReceived(Guid PayableId, Guid PaymentId, Guid? InvoiceId, DateTime PaymentDate) : INotification;
}