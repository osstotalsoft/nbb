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
            _repository = repository;
        }

        public async Task Handle(CreateContract request, CancellationToken cancellationToken)
        {
            var contract = new Contract(request.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(AddContractLine request, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(request.ContractId, cancellationToken);
            contract.AddContractLine(request.Product, request.Price, request.Quantity);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(ValidateContract request, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(request.ContractId, cancellationToken);
            contract.Validate();
            await _repository.SaveAsync(contract, cancellationToken);
        }
    }
}
