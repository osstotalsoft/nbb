// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Data.Abstractions;
using NBB.Payments.Domain.PayableAggregate;
using NBB.Payments.PublishedLanguage;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Payments.Application.CommandHandlers
{

    public class CreatePayableCommandHandler : IRequestHandler<CreatePayable>
    {
        private readonly ICrudRepository<Payable> _repository;

        public CreatePayableCommandHandler(ICrudRepository<Payable> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(CreatePayable command, CancellationToken cancellationToken)
        {
            var payable = new Payable(command.ClientId, command.Amount, command.InvoiceId, command.ContractId);
            await _repository.AddAsync(payable, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

}
