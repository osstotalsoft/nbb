using System;
using MediatR;

namespace NBB.Payments.Application.Commands
{
    public record CreatePayable(Guid ClientId, decimal Amount, Guid InvoiceId, Guid? ContractId);
    public record PayPayable(Guid PayableId) : IRequest;
}