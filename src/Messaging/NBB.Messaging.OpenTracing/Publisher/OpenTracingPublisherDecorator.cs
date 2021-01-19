using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Publisher
{
    public class OpenTracingPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITracer _tracer;
        private readonly ITopicRegistry _topicRegistry;

        public OpenTracingPublisherDecorator(IMessageBusPublisher inner, ITracer tracer,ITopicRegistry topicRegistry)
        {
            _inner = inner;
            _tracer = tracer;
            _topicRegistry = topicRegistry;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> customizer = null, string topicName = null)
        {
            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                if (_tracer.ActiveSpan != null)
                {
                    _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.TextMap,
                        new TextMapInjectAdapter(outgoingEnvelope.Headers));
                }

                customizer?.Invoke(outgoingEnvelope);
            }

            var formattedTopicName = _topicRegistry.GetTopicForName(topicName) ??
                               _topicRegistry.GetTopicForMessageType(message.GetType());
            var operationName = $"Publisher {message.GetType().GetPrettyName()}";

            using (var scope = _tracer.BuildSpan(operationName)
                .WithTag(Tags.Component, MessagingTags.ComponentMessaging)
                .WithTag(Tags.SpanKind, Tags.SpanKindProducer)
                .WithTag(Tags.MessageBusDestination, formattedTopicName)
                .WithTag(MessagingTags.CorrelationId, CorrelationManager.GetCorrelationId()?.ToString())
                .StartActive(true))
            {
                try
                {
                    return _inner.PublishAsync(message, cancellationToken, NewCustomizer, topicName);
                }
                catch (Exception exception)
                {
                    scope.Span.SetException(exception);
                    throw;
                }
            }
        }
    }
}
