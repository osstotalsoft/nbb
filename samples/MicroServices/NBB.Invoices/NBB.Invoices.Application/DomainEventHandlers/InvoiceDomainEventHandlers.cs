using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Invoices.Domain.InvoiceAggregate.DomainEvents;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Invoices.Application.DomainEventHandlers
{
    public class InvoiceDomainEventHandlers :
        INotificationHandler<InvoiceCreated>
    {

        private readonly IMessageBusPublisher _messageBusPublisher;

        public InvoiceDomainEventHandlers(IMessageBusPublisher messageBusPublisher)
        {
            _messageBusPublisher = messageBusPublisher;
        }

        public Task Handle(InvoiceCreated domainEvent, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.IntegrationEvents.InvoiceCreated(
                    domainEvent.InvoiceId, domainEvent.Amount, domainEvent.ClientId, domainEvent.ContractId),
                cancellationToken);
        }
    }
}
