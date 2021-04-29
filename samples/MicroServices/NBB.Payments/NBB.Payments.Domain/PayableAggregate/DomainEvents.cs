using System;
using MediatR;

namespace NBB.Payments.Domain.PayableAggregate
{
    public record PayableCreated(Guid PayableId, Guid? InvoiceId, Guid ClientId, Guid? ContractId, decimal Amount) : INotification;

    public record PaymentReceived
        (Guid PaymentId, Guid PayableId, Guid? InvoiceId, Guid? ContractId, DateTime PaymentDate) : INotification;
}