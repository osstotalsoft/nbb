// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;
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

        public async ValueTask Handle(InvoiceCreated domainEvent, CancellationToken cancellationToken)
        {
            await _messageBusPublisher.PublishAsync(
                new PublishedLanguage.InvoiceCreated(
                    domainEvent.InvoiceId, domainEvent.Amount, domainEvent.ClientId, domainEvent.ContractId),
                cancellationToken);
        }

        public async ValueTask Handle(InvoicePayed domainEvent, CancellationToken cancellationToken)
        {
            await _messageBusPublisher.PublishAsync(
                new PublishedLanguage.InvoiceMarkedAsPayed(
                    domainEvent.InvoiceId,domainEvent.ContractId),
                cancellationToken);
        }
    }
}
