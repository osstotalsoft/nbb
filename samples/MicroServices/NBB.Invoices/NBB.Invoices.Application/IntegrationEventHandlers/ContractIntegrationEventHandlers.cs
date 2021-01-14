using MediatR;
using NBB.Contracts.PublishedLanguage;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.IntegrationEventHandlers
{
    public class ContractIntegrationEventHandlers :
        INotificationHandler<ContractValidated>
    {
        private readonly ICrudRepository<Invoice> _invoiceRepository;

        public ContractIntegrationEventHandlers(ICrudRepository<Invoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task Handle(ContractValidated e, CancellationToken cancellationToken)
        {
            var invoice = new Invoice(e.ClientId, e.ContractId, e.Amount);
            await _invoiceRepository.AddAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
