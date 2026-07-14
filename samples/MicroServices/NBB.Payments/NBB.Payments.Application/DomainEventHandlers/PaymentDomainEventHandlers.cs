// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NBB.Messaging.Abstractions;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Application.DomainEventHandlers
{
    public class PaymentDomainEventHandlers :
        INotificationHandler<PaymentReceived>,
        INotificationHandler<PayableCreated>
    {
        private readonly IMessageBusPublisher _messageBusPublisher;

        public PaymentDomainEventHandlers(IMessageBusPublisher messageBusPublisher)
        {
            _messageBusPublisher = messageBusPublisher;
        }

        public async ValueTask Handle(PaymentReceived domainEvent, CancellationToken cancellationToken)
        {
            await _messageBusPublisher.PublishAsync(
                new PublishedLanguage.PaymentReceived(domainEvent.PayableId, domainEvent.PaymentId,
                    domainEvent.InvoiceId,
                    domainEvent.PaymentDate,
                    domainEvent.ContractId
                    ), cancellationToken);
        }

        public async ValueTask Handle(PayableCreated domainEvent, CancellationToken cancellationToken)
        {
             await _messageBusPublisher.PublishAsync(
                new PublishedLanguage.PayableCreated(domainEvent.PayableId, domainEvent.InvoiceId,
                    domainEvent.ClientId,
                    domainEvent.Amount,
                    domainEvent.ContractId
                    ), cancellationToken);
        }
    }
}
