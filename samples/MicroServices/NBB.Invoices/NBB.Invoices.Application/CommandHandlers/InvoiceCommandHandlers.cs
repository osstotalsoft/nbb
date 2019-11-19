using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Application.Commands;
using NBB.Invoices.Domain.InvoiceAggregate;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.CommandHandlers
{
    public class InvoiceCommandHandlers : IRequestHandler<CreateInvoice>
    {
        private readonly ICrudRepository<Invoice> _repository;
        public InvoiceCommandHandlers(ICrudRepository<Invoice> repository)
        {
            this._repository = repository;
        }

        public async Task Handle(CreateInvoice command, CancellationToken cancellationToken)
        {
            var invoice = new Invoice(command.ClientId, command.ContractId, command.Amount);
            await _repository.AddAsync(invoice, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

        }
    }
}
