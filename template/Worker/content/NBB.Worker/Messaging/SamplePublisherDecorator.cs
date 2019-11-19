#if AddSamples
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Messaging
{
    public class SamplePublisherDecorator: IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ILogger<SamplePublisherDecorator> _logger;

        public SamplePublisherDecorator(IMessageBusPublisher inner, ILogger<SamplePublisherDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> customizer = null, string topicName = null)
        {
            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
               
                outgoingEnvelope.SetHeader("SampleHeader", "SampleValue");
                customizer?.Invoke(outgoingEnvelope);
            }

            _logger.LogDebug("Message {@Message} was sent.", message);
            return _inner.PublishAsync(message, cancellationToken, NewCustomizer);
        }
    }
}
#endif