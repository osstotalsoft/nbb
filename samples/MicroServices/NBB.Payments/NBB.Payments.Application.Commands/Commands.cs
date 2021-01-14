using System;
using MediatR;

namespace NBB.Payments.Application.Commands
{
    public record PayPayable(Guid PayableId) : IRequest;
}