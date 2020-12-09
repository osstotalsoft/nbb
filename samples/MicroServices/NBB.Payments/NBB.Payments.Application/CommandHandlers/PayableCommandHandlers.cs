using MediatR;
using NBB.Data.Abstractions;
using NBB.Payments.Application.Commands;
using NBB.Payments.Domain.PayableAggregate;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Payments.Application.CommandHandlers
{
    public class PayableCommandHandlers : IRequestHandler<PayPayable>
    {
        private readonly ICrudRepository<Payable> _repository;

        public PayableCommandHandlers(ICrudRepository<Payable> repository)
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
