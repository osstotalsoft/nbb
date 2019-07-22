using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Application.DataContracts;
using NBB.Messaging.Abstractions;

namespace NBB.Contracts.Application.CommandHandlers
{
    public class MessageBusPublisherCommandHandler : IRequestHandler<Command>
    {
        private readonly IMessageBusPublisher _messageBusPublisher;

        public MessageBusPublisherCommandHandler(IMessageBusPublisher messageBusPublisher)
        {
            _messageBusPublisher = messageBusPublisher;
        }

        public async Task Handle(Command message, CancellationToken cancellationToken)
        {
            await _messageBusPublisher.PublishAsync(message, cancellationToken);
        }
    }
}
