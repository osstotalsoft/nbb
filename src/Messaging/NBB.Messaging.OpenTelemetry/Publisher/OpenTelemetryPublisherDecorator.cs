// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using NBB.Core.Abstractions;

namespace NBB.Messaging.OpenTelemetry.Publisher
{
    public class OpenTelemetryPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITopicRegistry _topicRegistry;

        private static readonly ActivitySource activitySource = MessagingActivitySource.Current;
        private static readonly TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

        public OpenTelemetryPublisherDecorator(IMessageBusPublisher inner, ITopicRegistry topicRegistry)
        {
            _inner = inner;
            _topicRegistry = topicRegistry;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= MessagingPublisherOptions.Default;

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {

                if (Activity.Current != null)
                {
                    var contextToInject = Activity.Current.Context;
                    propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), outgoingEnvelope.Headers, (headers, key, value) => headers[key] = value);
                }

                options.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            var formattedTopicName = _topicRegistry.GetTopicForName(options.TopicName) ??
                                     _topicRegistry.GetTopicForMessageType(message.GetType());
            var operationName = $"{message.GetType().GetPrettyName()} send";

            using var activity = activitySource.StartActivity(operationName, ActivityKind.Producer);

            activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestination, formattedTopicName);
            activity?.SetTag(TracingTags.CorrelationId, Correlation.CorrelationManager.GetCorrelationId()?.ToString());

            try
            {
                await _inner.PublishAsync(message, options with { EnvelopeCustomizer = NewCustomizer },
                    cancellationToken);

            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.AddException(exception);
                throw;
            }
        }
    }
}
