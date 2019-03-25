using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Publisher
{
    public class OpenTracingPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITracer _tracer;

        public OpenTracingPublisherDecorator(IMessageBusPublisher inner, ITracer tracer)
        {
            _inner = inner;
            _tracer = tracer;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken, Action<MessagingEnvelope> customizer = null, string topicName = null)
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

            string operationName = $"Publisher {message.GetType().GetPrettyName()}";
            using (var scope = _tracer.BuildSpan(operationName)
                .WithTag(Tags.Component, "NBB.Messaging.Publisher")
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag("correlationId", CorrelationManager.GetCorrelationId()?.ToString())
                .StartActive(finishSpanOnDispose: true))
            {
                try
                {
                    return _inner.PublishAsync(message, cancellationToken, NewCustomizer);
                }
                catch (Exception exception)
                {
                    scope.Span.Log(new Dictionary<string, object>(3)
                    {
                        { LogFields.Event, Tags.Error.Key },
                        { LogFields.ErrorKind, exception.GetType().Name },
                        { LogFields.ErrorObject, exception }
                    });
                
                    scope.Span.SetTag(Tags.Error, true);

                    throw;
                }
            }
        }
    }
}
