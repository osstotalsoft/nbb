using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public class MessageBusSubscriberService<TMessage> : BackgroundService
    {
        private readonly MessagingSubscriberOptions _subscriberOptions;
        private readonly IMessageBusSubscriber<TMessage> _messageBusSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingContextAccessor _messagingContextAccessor;
        private readonly ILogger<MessageBusSubscriberService<TMessage>> _logger;

        public MessageBusSubscriberService(
            IMessageBusSubscriber<TMessage> messageBusSubscriber, 
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is starting", typeof(TMessage).GetPrettyName());

            Task HandleMsg(MessagingEnvelope<TMessage> msg) => Handle(msg, cancellationToken);

            await _messageBusSubscriber.SubscribeAsync(HandleMsg, cancellationToken, null, _subscriberOptions);
            await cancellationToken.WhenCanceled();
            await _messageBusSubscriber.UnSubscribeAsync(HandleMsg, CancellationToken.None);

            _logger.LogInformation("MessageBusSubscriberService for message type {MessageType} is stopping", typeof(TMessage).GetPrettyName());
        }


        private async Task Handle(MessagingEnvelope<TMessage> message, CancellationToken cancellationToken)
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
