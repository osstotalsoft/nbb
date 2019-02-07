using MediatR;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// A pipeline middleware that sends/publishes messages that are events, commands or queries to mediatR.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{NBB.Messaging.DataContracts.MessagingEnvelope}" />
    public class MediatRMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly IMediator _mediator;
        private readonly IMessageBusPublisher _messageBusPublisher;

        public MediatRMiddleware(IMediator mediator, IMessageBusPublisher messageBusPublisher)
        {
            _mediator = mediator;
            _messageBusPublisher = messageBusPublisher;
        }

        public async Task Invoke(MessagingEnvelope messageContext, CancellationToken cancellationToken, Func<Task> next)
        {
            if (messageContext.Payload is IEvent @event)
            {
                await _mediator.Publish(@event, cancellationToken);
            }
            else if (messageContext.Payload is ICommand command)
            {
                await _mediator.Send(command, cancellationToken);
            }
            else if (messageContext.Payload is IQuery query)
            {
                var sendMethod = _mediator.GetType().GetMethods()
                    .Single(m => m.IsGenericMethod && m.Name == nameof(Mediator.Send))
                    .MakeGenericMethod(query.GetResponseType());

                var queryResult = await (dynamic)sendMethod.Invoke(_mediator, new object[] { messageContext.Payload, cancellationToken });

                var messagingResponseType = typeof(MessagingResponse<>).MakeGenericType(query.GetResponseType());
                var messagingResponse = Activator.CreateInstance(messagingResponseType, queryResult, query.QueryId) as IMessagingResponse;
                await _messageBusPublisher.PublishAsync(messagingResponse, cancellationToken);
            }
            else
            {
                throw new ApplicationException($"Message type {messageContext.Payload.GetType()} cannot be handled by mediatR");
            }

            await next();
        }
    }
}
