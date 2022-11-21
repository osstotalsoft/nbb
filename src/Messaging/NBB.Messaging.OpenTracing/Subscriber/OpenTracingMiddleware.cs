// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Subscriber
{
    public class OpenTracingMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ITracer _tracer;

        public OpenTracingMiddleware(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            var extractedSpanContext = _tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(context.MessagingEnvelope.Headers));
            string operationName = $"Subscriber {context.MessagingEnvelope.Payload.GetType().GetPrettyName()}";

            using var scope = _tracer.BuildSpan(operationName)
                .AddReference(References.FollowsFrom, extractedSpanContext)
                .WithTag(Tags.Component, "NBB.Messaging")
                .WithTag(Tags.SpanKind, Tags.SpanKindConsumer)
                .WithTag(Tags.PeerService,
                    context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var value)
                        ? value
                        : default)
                .WithTag(MessagingTags.CorrelationId, CorrelationManager.GetCorrelationId()?.ToString())
                .WithTag(Tags.SamplingPriority, 1)
                .StartActive(true);

            foreach (var header in context.MessagingEnvelope.Headers)
                scope.Span.SetTag(MessagingTags.MessagingEnvelopeHeaderSpanTagPrefix + header.Key.ToLower(), header.Value);

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
