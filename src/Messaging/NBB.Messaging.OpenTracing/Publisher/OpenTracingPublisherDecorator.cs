// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTracing.Publisher
{
    public class OpenTracingPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITopicRegistry _topicRegistry;

        public OpenTracingPublisherDecorator(IMessageBusPublisher inner, ITopicRegistry topicRegistry)
        {
            _inner = inner;
            _topicRegistry = topicRegistry;
        }

        private static ActivitySource activitySource = new(MessagingTags.ComponentMessaging); //, System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString());
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= MessagingPublisherOptions.Default;

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {

                if (Activity.Current != null)
                {
                    var contextToInject = Activity.Current.Context;
                    Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), outgoingEnvelope.Headers, (headers, key, value) => headers[key] = value);
                 }
              
                options.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            var formattedTopicName = _topicRegistry.GetTopicForName(options.TopicName) ??
                                     _topicRegistry.GetTopicForMessageType(message.GetType());
            var operationName = $"Publisher {message.GetType().GetPrettyName()}";

                              
            using var activity = activitySource.StartActivity(operationName, ActivityKind.Producer);
            //activity?.SetTag(TraceSemanticConventions.AttributeMessagingSystem, "nats");
            activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestination, formattedTopicName);
            activity?.SetTag(MessagingTags.CorrelationId, Correlation.CorrelationManager.GetCorrelationId()?.ToString());

            //.WithTag(Tags.SamplingPriority, 1)

            try
            {
                await _inner.PublishAsync(message, options with { EnvelopeCustomizer = NewCustomizer },
                    cancellationToken);

            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message );
                activity?.RecordException(exception);
                throw;
            }
        }
    }
}
