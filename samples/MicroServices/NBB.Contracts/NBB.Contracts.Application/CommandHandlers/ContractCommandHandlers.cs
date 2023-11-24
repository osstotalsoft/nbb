// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ContractDomainMetrics _domainMetrics;
        private readonly ILogger<ContractCommandHandlers> _logger;

        public ContractCommandHandlers(IEventSourcedRepository<Contract> repository, ContractDomainMetrics domainMetrics, ILogger<ContractCommandHandlers> logger)
        {
            this._repository = repository;
            _domainMetrics = domainMetrics;
            _logger = logger;
        }

        public async Task Handle(CreateContract command, CancellationToken cancellationToken)
        {
            var contract = new Contract(command.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);
            _domainMetrics.ContractCreated();
        }

        public async Task Handle(AddContractLine command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.AddContractLine(command.Product, command.Price, command.Quantity);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(ValidateContract command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating contract");

            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.Validate();
            await _repository.SaveAsync(contract, cancellationToken);
            _domainMetrics.ContractValidated();
        }
    }
}
