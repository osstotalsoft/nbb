// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.OpenTracing.Publisher;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Subscriber
{
    public class OpenTracingMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private static readonly AssemblyName assemblyName = typeof(OpenTracingMiddleware).Assembly.GetName();
        private static readonly ActivitySource ActivitySource = new(assemblyName.Name, assemblyName.Version.ToString());
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            var parentContext = Propagator.Extract(default, context.MessagingEnvelope.Headers,
                (headers, key) => headers.TryGetValue(key, out var value) ? new[] { value } : Enumerable.Empty<string>());

            Baggage.Current = parentContext.Baggage;

            string activityName = $"{context.MessagingEnvelope.Payload.GetType().GetPrettyName()} receive";

            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
            activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestination, context.TopicName);
            activity?.SetTag(MessagingTags.CorrelationId, Correlation.CorrelationManager.GetCorrelationId()?.ToString());
            activity?.SetTag(TraceSemanticConventions.AttributePeerService, context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var value)
                ? value
                : default);

            foreach (var header in context.MessagingEnvelope.Headers)
                activity?.SetTag(MessagingTags.MessagingEnvelopeHeaderSpanTagPrefix + header.Key.ToLower(), header.Value);

            try
            {
                await next();
            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.RecordException(exception);
                throw;
            }
        }
    }
}
