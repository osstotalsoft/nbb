using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.PublishedLanguage.IntegrationEvents;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Application.IntegrationEventHandlers
{
    public class InvoiceIntegrationEventHandlers : INotificationHandler<InvoiceCreated>
    {
        private readonly ICrudRepository<Payable> _repository;

        public InvoiceIntegrationEventHandlers(ICrudRepository<Payable> repository)
        {
            this._repository = repository;
        }

        public async Task Handle(InvoiceCreated e, CancellationToken cancellationToken)
        {
            var payable = new Payable(e.ClientId, e.Amount, e.InvoiceId);
            await _repository.AddAsync(payable);
            await _repository.SaveChangesAsync();
        }
    }
}
