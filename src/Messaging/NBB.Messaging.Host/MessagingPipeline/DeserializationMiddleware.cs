using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NBB.Messaging.Host.MessagingPipeline
{
    public class DeserializationMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly IMessageSerDes _messageSerDes;
        private MessagingSubscriberOptions _subscriberOptions;

        public DeserializationMiddleware(MessagingContextAccessor messagingContextAccessor, IMessageSerDes messageSerDes, MessagingSubscriberOptions subscriberOptions)
        {
            _messagingContextAccessor = messagingContextAccessor;
            _messageSerDes = messageSerDes;
            _subscriberOptions = subscriberOptions;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var payload = _subscriberOptions.SerDes.DeserializationType == DeserializationType.HeadersOnly
                ? message.Payload.ToString()
                : ((JObject)message.Payload).ToObject(_messagingContextAccessor.MessagingContext.PayloadType);

            //_messagingContextAccessor.MessagingContext = new MessagingContext(new MessagingEnvelope(message.Headers, payload), _messagingContextAccessor.MessagingContext.PayloadType);

            await next();
        }
    }
}
