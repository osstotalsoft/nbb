using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Messaging.Abstractions;

namespace NBB.Invoices.Application.DomainEventHandlers
{
    public class InvoiceDomainEventHandlers :
        INotificationHandler<InvoiceCreated>,
        INotificationHandler<InvoicePayed>
    {

        private readonly IMessageBusPublisher _messageBusPublisher;

        public InvoiceDomainEventHandlers(IMessageBusPublisher messageBusPublisher)
        {
            _messageBusPublisher = messageBusPublisher;
        }

        public Task Handle(InvoiceCreated domainEvent, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.InvoiceCreated(
                    domainEvent.InvoiceId, domainEvent.Amount, domainEvent.ClientId, domainEvent.ContractId),
                cancellationToken);
        }

        public Task Handle(InvoicePayed domainEvent, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.InvoiceMarkedAsPayed(
                    domainEvent.InvoiceId,domainEvent.ContractId),
                cancellationToken);
        }
    }
}
