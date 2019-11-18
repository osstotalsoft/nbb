using MediatR;
using NBB.Contracts.Application.Commands;
using NBB.Contracts.Domain.ContractAggregate;
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

        public async Task Handle(CreateContract command, CancellationToken cancellationToken)
        {
            var contract = new Contract(command.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(AddContractLine command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.AddContractLine(command.Product, command.Price, command.Quantity);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(ValidateContract command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.Validate();
            await _repository.SaveAsync(contract, cancellationToken);
        }
    }
}
