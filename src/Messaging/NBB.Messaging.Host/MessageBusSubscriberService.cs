using System;
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
    public class MessageBusSubscriberService<TMessage> : BackgroundService
    {
        private readonly MessagingSubscriberOptions _subscriberOptions;
        private readonly IMessageBusSubscriber _messageBusSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly ILogger<MessageBusSubscriberService<TMessage>> _logger;

        public MessageBusSubscriberService(
            IMessageBusSubscriber messageBusSubscriber, 
            IServiceProvider serviceProvider,
            MessagingContextAccessor messagingContextAccessor,
            ILogger<MessageBusSubscriberService<TMessage>> logger,
            MessagingSubscriberOptions subscriberOptions = null
            )
        {
            _subscriberOptions = subscriberOptions;
            _messageBusSubscriber = messageBusSubscriber;
            _serviceProvider = serviceProvider;
            _messagingContextAccessor = messagingContextAccessor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is starting", typeof(TMessage).GetPrettyName());

            Task HandleMsg(MessagingEnvelope msg) => Handle(msg, stoppingToken);

            var subs = await _messageBusSubscriber.SubscribeAsync<TMessage>(HandleMsg, stoppingToken, null, _subscriberOptions);
            await stoppingToken.WhenCanceled();
            subs.Dispose();

            _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is stopping", typeof(TMessage).GetPrettyName());
        }


        private async Task Handle(MessagingEnvelope message, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pipeline = scope.ServiceProvider.GetService<PipelineDelegate<MessagingEnvelope>>();
                _messagingContextAccessor.MessagingContext = new MessagingContext(message);
                await pipeline(message, cancellationToken);
            }
        }
    }
}
