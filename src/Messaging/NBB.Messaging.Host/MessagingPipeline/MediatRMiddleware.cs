using MediatR;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// A pipeline middleware that forwards messages that contain requests or events to mediatR.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class MediatRMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly IMediator _mediator;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private MessagingSubscriberOptions _subscriberOptions;

        public MediatRMiddleware(IMediator mediator, MessagingContextAccessor messagingContextAccessor, MessagingSubscriberOptions subscriberOptions)
        {
            _mediator = mediator;
            _messagingContextAccessor = messagingContextAccessor;
            _subscriberOptions = subscriberOptions;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var payload = _subscriberOptions.SerDes.DeserializationType == DeserializationType.HeadersOnly
                ? message.Payload.ToString()
                : ((JObject)message.Payload).ToObject(_messagingContextAccessor.MessagingContext.PayloadType);

            // maybe
            _messagingContextAccessor.MessagingContext = new MessagingContext(new MessagingEnvelope(message.Headers, payload), _messagingContextAccessor.MessagingContext.PayloadType, _messagingContextAccessor.MessagingContext.TopicName);

            if (payload is INotification @event)
            {
                await _mediator.Publish(@event, cancellationToken);
            }
            else if (payload is IRequest request)
            {
                await _mediator.Send(request, cancellationToken);
            }
            else
            {
                throw new ApplicationException($"Message type {message.Payload.GetType()} cannot be handled by mediatR");
            }

            await next();
        }
    }
}
