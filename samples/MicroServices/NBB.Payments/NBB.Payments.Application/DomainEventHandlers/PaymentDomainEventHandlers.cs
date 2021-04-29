using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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

        public Task Handle(PaymentReceived domainEvent, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.PaymentReceived(domainEvent.PayableId, domainEvent.PaymentId,
                    domainEvent.InvoiceId,
                    domainEvent.PaymentDate,
                    domainEvent.ContractId
                    ), cancellationToken);
        }

        public Task Handle(PayableCreated domainEvent, CancellationToken cancellationToken)
        {
             return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.PayableCreated(domainEvent.PayableId, domainEvent.InvoiceId,
                    domainEvent.ClientId,
                    domainEvent.Amount,
                    domainEvent.ContractId
                    ), cancellationToken);
        }
    }
}
