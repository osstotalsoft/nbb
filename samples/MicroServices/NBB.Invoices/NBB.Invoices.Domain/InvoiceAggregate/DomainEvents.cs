using System;
using MediatR;

namespace NBB.Invoices.Domain.InvoiceAggregate
{
    public record InvoiceCreated(Guid InvoiceId, decimal Amount, Guid ClientId, Guid? ContractId) : INotification;

    public record InvoicePayed(Guid InvoiceId, Guid PaymentId, Guid? ContractId) : INotification;
}