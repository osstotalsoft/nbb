// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.Internal
{
    internal class HostedSubscriber<TMessage> : IHostedSubscriber
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly ILogger<MessagingHost> _logger;
        private readonly ITopicRegistry _topicRegistry;
        private readonly ExecutionMonitor _executionMonitor;

        public HostedSubscriber(
            IMessageBus messageBus,
            IServiceProvider serviceProvider,
            MessagingContextAccessor messagingContextAccessor,
            ILogger<MessagingHost> logger,
            ITopicRegistry topicRegistry,
            ExecutionMonitor executionMonitor
        )
        {
            _messageBus = messageBus;
            _serviceProvider = serviceProvider;
            _messagingContextAccessor = messagingContextAccessor;
            _logger = logger;
            _topicRegistry = topicRegistry;
            _executionMonitor = executionMonitor;
        }

        public async Task<HostedSubscription> SubscribeAsync(PipelineDelegate<MessagingContext> pipeline,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var topicName = _topicRegistry.GetTopicForName(options?.TopicName, false) ??
                            _topicRegistry.GetTopicForMessageType(typeof(TMessage), false);

            Task HandleMsg(MessagingEnvelope<TMessage> msg) => _executionMonitor.Handle(() => Handle(msg, topicName, pipeline, cancellationToken));

            _logger.LogInformation("Messaging subscriber for topic {TopicName} is starting", topicName);

            var subscription = await _messageBus.SubscribeAsync<TMessage>(HandleMsg, options, cancellationToken);

            return new HostedSubscription(subscription, topicName, _logger);
        }

        private async Task Handle(MessagingEnvelope<TMessage> message, string topicName,
            PipelineDelegate<MessagingContext> pipeline, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = new MessagingContext(message, topicName, scope.ServiceProvider);
            _messagingContextAccessor.MessagingContext = context;

            await pipeline(context, cancellationToken);
        }
    }
}
