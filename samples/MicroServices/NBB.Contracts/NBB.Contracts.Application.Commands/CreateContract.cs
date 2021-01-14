using System;
using MediatR;

namespace NBB.Contracts.Application.Commands
{
    public record CreateContract(
        Guid ClientId
    ) : IRequest;
}