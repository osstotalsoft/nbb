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
    public class PayPayableCommandHandler : IRequestHandler<PayPayable>
    {
        private readonly ICrudRepository<Payable> _repository;

        public PayPayableCommandHandler(ICrudRepository<Payable> repository)
        {
            _repository = repository;
        }


        public async Task<Unit> Handle(PayPayable command, CancellationToken cancellationToken)
        {
            var payable = await _repository.GetByIdAsync(command.PayableId, cancellationToken);
            if (payable != null)
            {
                payable.Pay();
                await _repository.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
