using System;
using MediatR;

namespace NBB.Contracts.Application.Commands
{
    public record ValidateContract(
        Guid ContractId
    ) : IRequest;
    //string IKeyProvider.Key => ContractId.ToString();
}
