using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.CommandHandlers
{
    public class MarkInvoiceAsPayedCommandHandler : IRequestHandler<MarkInvoiceAsPayed>
    {
        private readonly ICrudRepository<Invoice> _repository;
        public MarkInvoiceAsPayedCommandHandler(ICrudRepository<Invoice> repository)
        {
            this._repository = repository;
        }

        public async Task<Unit> Handle(MarkInvoiceAsPayed command, CancellationToken cancellationToken)
        {
            var invoice = await _repository.GetByIdAsync(command.InvoiceId, cancellationToken);
            if (invoice != null)
            {
                invoice.MarkAsPayed(command.PaymentId, invoice.ContractId);

                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
    }
}
