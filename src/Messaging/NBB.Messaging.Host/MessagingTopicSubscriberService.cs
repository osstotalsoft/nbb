using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Host
{
    public class MessagingTopicSubscriberService : BackgroundService
    {
        private readonly IMessagingTopicSubscriber _messagingTopicSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageSerDes _messageSerDes;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly ILogger<MessagingTopicSubscriberService> _logger;
        private readonly ITopicRegistry _topicRegistry;
        private readonly string _topic;
        private readonly MessagingSubscriberOptions _subscriberOptions;

        public MessagingTopicSubscriberService(
            string topic,
            IMessageSerDes messageSerDes,
            IMessagingTopicSubscriber messagingTopicSubscriber, 
            IServiceProvider serviceProvider,
            MessagingContextAccessor messagingContextAccessor,
            ITopicRegistry topicRegistry,
            ILogger<MessagingTopicSubscriberService> logger,
            MessagingSubscriberOptions subscriberOptions = null)
        {
            _messagingTopicSubscriber = messagingTopicSubscriber;
            _serviceProvider = serviceProvider;
            _messagingContextAccessor = messagingContextAccessor;
            _messageSerDes = messageSerDes;
            _logger = logger;
            _subscriberOptions = subscriberOptions;
            _topicRegistry = topicRegistry;
            _topic = _topicRegistry.GetTopicForName(topic);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"MessagingTopicSubscriberService for topic {_topic} is starting");

            Task HandleMsg(string msg) => Handle(msg, cancellationToken);

            await _messagingTopicSubscriber.SubscribeAsync(_topic, HandleMsg, cancellationToken, _subscriberOptions);
            await cancellationToken.WhenCanceled();
            await _messagingTopicSubscriber.UnSubscribeAsync(cancellationToken);

            _logger.LogInformation($"MessagingTopicSubscriberService for topic {_topic} is stopping");
        }


        private async Task Handle(string message, CancellationToken cancellationToken)
        {
            MessagingEnvelope messageEnvelope = null;
            try
            {
                messageEnvelope = _messageSerDes.DeserializeMessageEnvelope(message, _subscriberOptions?.SerDes);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "MessagingTopicSubscriberService encountered an error when deserializing a message from topic {TopicName}.\n {Error}",
                    _topic, ex);
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var pipeline = scope.ServiceProvider.GetService<PipelineDelegate<MessagingEnvelope>>();
                _messagingContextAccessor.MessagingContext = new MessagingContext(messageEnvelope);
                await pipeline(messageEnvelope, cancellationToken);
            }
        }
    }
}
