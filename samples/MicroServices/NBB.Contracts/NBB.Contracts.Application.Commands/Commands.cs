using System;
using MediatR;

namespace NBB.Contracts.Application.Commands
{
    public record AddContractLine(string Product, decimal Price, int Quantity, Guid ContractId) : IRequest;

    public record CreateContract(Guid ClientId) : IRequest;

    public record ValidateContract(Guid ContractId) : IRequest;
}