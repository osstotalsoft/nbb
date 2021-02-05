using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public class MessageBusSubscriberService<TMessage> : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly MessagingSubscriberOptions _subscriberOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly ILogger<MessageBusSubscriberService<TMessage>> _logger;
        private readonly ITopicRegistry _topicRegistry;
        private readonly Action<IPipelineBuilder<MessagingEnvelope>> _pipelineConfigurator;

        public MessageBusSubscriberService(
            IMessageBus messageBus,
            IServiceProvider serviceProvider,
            MessagingContextAccessor messagingContextAccessor,
            ILogger<MessageBusSubscriberService<TMessage>> logger,
            ITopicRegistry topicRegistry,
            Action<IPipelineBuilder<MessagingEnvelope>> pipelineConfigurator,
            MessagingSubscriberOptions subscriberOptions = null
        )
        {
            _messageBus = messageBus;
            _subscriberOptions = subscriberOptions;
            _serviceProvider = serviceProvider;
            _messagingContextAccessor = messagingContextAccessor;
            _logger = logger;
            _topicRegistry = topicRegistry;
            _pipelineConfigurator = pipelineConfigurator;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Task HandleMsg(MessagingEnvelope<TMessage> msg) => Handle(msg, cancellationToken);

            OnStarting();

            using var subscription =
                await _messageBus.SubscribeAsync<TMessage>(HandleMsg, _subscriberOptions, cancellationToken);

            await cancellationToken.WhenCanceled();

            OnStopping();
        }

        protected virtual void OnStarting()
            => _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is starting",
                typeof(TMessage).GetPrettyName());

        protected virtual void OnStopping()
            => _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is stopping",
                typeof(TMessage).GetPrettyName());

        private async Task Handle(MessagingEnvelope<TMessage> message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var topicName = _topicRegistry.GetTopicForName(_subscriberOptions?.TopicName, false) ??
                            _topicRegistry.GetTopicForMessageType(typeof(TMessage), false);

            var pipelineBuilder = new PipelineBuilder<MessagingEnvelope>(scope.ServiceProvider);
            _pipelineConfigurator.Invoke(pipelineBuilder);
            var pipeline = pipelineBuilder.Pipeline;

            _messagingContextAccessor.MessagingContext = new MessagingContext(message, topicName);

            await pipeline(message, cancellationToken);
        }
    }

    public class MessageBusSubscriberService : MessageBusSubscriberService<object>
    {
        private readonly ILogger<MessageBusSubscriberService<object>> _logger;
        private readonly MessagingSubscriberOptions _subscriberOptions;

        public MessageBusSubscriberService(
            IMessageBus messageBus,
            IServiceProvider serviceProvider,
            MessagingContextAccessor messagingContextAccessor,
            ILogger<MessageBusSubscriberService<object>> logger,
            ITopicRegistry topicRegistry,
            Action<IPipelineBuilder<MessagingEnvelope>> pipelineConfigurator,
            MessagingSubscriberOptions subscriberOptions)
            : base(messageBus, serviceProvider, messagingContextAccessor, logger, topicRegistry, pipelineConfigurator, subscriberOptions)
        {
            _logger = logger;
            _subscriberOptions = subscriberOptions;
        }

        protected override void OnStarting()
            => _logger.LogInformation(
                "MessageBusSubscriberService for topic {TopicName} is starting", _subscriberOptions.TopicName);

        protected override void OnStopping()
            => _logger.LogInformation(
                "MessageBusSubscriberService for topic {TopicName} is stopping", _subscriberOptions.TopicName);
    }
}