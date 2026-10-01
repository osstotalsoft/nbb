using System;
using Mediator;

namespace NBB.Payments.Application.Commands
{
    public record CreatePayable(Guid ClientId, decimal Amount, Guid InvoiceId, Guid? ContractId) : IRequest;
    public record PayPayable(Guid PayableId) : IRequest;
}