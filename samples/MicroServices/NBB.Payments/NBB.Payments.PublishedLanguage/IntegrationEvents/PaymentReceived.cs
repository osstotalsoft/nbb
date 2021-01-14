using System;
using MediatR;

namespace NBB.Payments.PublishedLanguage.IntegrationEvents
{
    public record PaymentReceived(
        Guid PayableId,
        Guid PaymentId,
        Guid? InvoiceId,
        DateTime PaymentDate
    ) : INotification;
}