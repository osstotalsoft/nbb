using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Contracts.Domain.ContractAggregate.DomainEvents;
using NBB.Messaging.Abstractions;

namespace NBB.Contracts.Application.DomainEventHandlers
{
    public class ContractDomainEventHandlers :
        INotificationHandler<ContractValidated>
    {
        private readonly IMessageBusPublisher _messageBusPublisher;

        public ContractDomainEventHandlers(IMessageBusPublisher messageBusPublisher)
        {
            _messageBusPublisher = messageBusPublisher;
        }

        public Task Handle(ContractValidated domainEvent, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(
                new PublishedLanguage.IntegrationEvents.ContractValidated(domainEvent.ContractId, domainEvent.ClientId, domainEvent.Amount), cancellationToken);
        }
    }
}
