// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Contracts.Domain.ContractAggregate;
using NBB.Contracts.PublishedLanguage;
using NBB.Data.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Application.CommandHandlers
{
    public class ContractCommandHandlers :
        IRequestHandler<CreateContract>,
        IRequestHandler<AddContractLine>,
        IRequestHandler<ValidateContract>
    {
        private readonly IEventSourcedRepository<Contract> _repository;

        public ContractCommandHandlers(IEventSourcedRepository<Contract> repository)
        {
            this._repository = repository;
        }

        public async Task<Unit> Handle(CreateContract command, CancellationToken cancellationToken)
        {
            var contract = new Contract(command.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(AddContractLine command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.AddContractLine(command.Product, command.Price, command.Quantity);
            await _repository.SaveAsync(contract, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(ValidateContract command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.Validate();
            await _repository.SaveAsync(contract, cancellationToken);

            return Unit.Value;
        }
    }
}
