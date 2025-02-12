// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTelemetry.Subscriber
{
    public class OpenTelemetrySubscriberDecorator : IMessageBusSubscriber
    {
        private readonly IMessageBusSubscriber _inner;

        private static readonly ActivitySource activitySource = MessagingActivitySource.Current;
        private static readonly TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

        public OpenTelemetrySubscriberDecorator(IMessageBusSubscriber inner)
        {
            _inner = inner;
        }

        public Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler, MessagingSubscriberOptions options = null, CancellationToken cancellationToken = default)
        {
            async Task NewHandler(MessagingEnvelope<TMessage> incommingEnvelope)
            {
                var parentContext = propagator.Extract(default, incommingEnvelope.Headers,
                    (headers, key) => headers.TryGetValue(key, out var value) ? new[] { value } : Enumerable.Empty<string>());
                Baggage.Current = parentContext.Baggage;

                string activityName = $"{incommingEnvelope.Payload.GetType().GetPrettyName()} receive";

                using var activity = activitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
                activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestination, options.TopicName);
                activity?.SetTag(TraceSemanticConventions.AttributePeerService, incommingEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var value)
                    ? value
                    : default);

                foreach (var header in incommingEnvelope.Headers)
                    activity?.SetTag(TracingTags.MessagingEnvelopeHeaderSpanTagPrefix + header.Key.ToLower(), header.Value);

                try
                {
                    await handler?.Invoke(incommingEnvelope);
                }
                catch (Exception exception)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                    activity?.AddException(exception);
                    throw;
                }
            }

            return _inner.SubscribeAsync((Func<MessagingEnvelope<TMessage>, Task>)NewHandler, options, cancellationToken);
        }
    }
}
