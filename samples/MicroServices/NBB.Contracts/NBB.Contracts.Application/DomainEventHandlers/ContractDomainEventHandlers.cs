// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Contracts.Domain.ContractAggregate;
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
                new PublishedLanguage.ContractValidated(domainEvent.ContractId, domainEvent.ClientId, domainEvent.Amount), cancellationToken);
        }
    }
}
