using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Subscriber
{
    public class OpenTracingMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ITracer _tracer;

        public OpenTracingMiddleware(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var extractedSpanContext = _tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(message.Headers));
            string operationName = $"Subscriber {message.Payload.GetType().GetPrettyName()}";

            using (var scope = _tracer.BuildSpan(operationName)
                .AddReference(References.FollowsFrom, extractedSpanContext)
                .WithTag(Tags.Component, "NBB.Messaging")
                .WithTag(Tags.SpanKind, Tags.SpanKindConsumer)
                .WithTag(Tags.PeerService,
                    message.Headers.TryGetValue(MessagingHeaders.Source, out var value) ? value : default)
                .WithTag("correlationId", CorrelationManager.GetCorrelationId()?.ToString())
                .StartActive(true))
            {

                try
                {
                    await next();
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
