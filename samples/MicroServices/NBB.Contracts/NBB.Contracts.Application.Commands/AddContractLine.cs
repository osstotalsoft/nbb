using System;
using MediatR;

namespace NBB.Contracts.Application.Commands
{
    public record AddContractLine(
        string Product,
        decimal Price,
        int Quantity,
        Guid ContractId
    ) : IRequest;
}