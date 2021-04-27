using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Messaging.Abstractions;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Application.DomainEventHandlers
{
    public class PaymentDomainEventHandlers :
        INotificationHandler<PaymentReceived>
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
                    Guid.NewGuid() // TODO: pass correct contractId
                    ), cancellationToken);
        }
    }
}
