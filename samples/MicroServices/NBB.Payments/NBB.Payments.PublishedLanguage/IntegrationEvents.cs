using System;
using MediatR;

namespace NBB.Payments.PublishedLanguage
{
    public record PaymentReceived(Guid PayableId, Guid PaymentId, Guid? InvoiceId, DateTime PaymentDate, Guid? ContractId) : INotification;
    public record PayableCreated(Guid PayableId, Guid? InvoiceId, Guid ClientId, decimal Amount, Guid? ContractId) : INotification;

}