﻿using NBB.Correlation;
using NBB.EventStore.Abstractions;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.MessagingExtensions
{
    public class MessagingEventStoreSubscriber : IEventStoreSubscriber
    {
        private readonly IMessageBusSubscriber<object> _messageBusSubscriber;
        private readonly MessagingTopicResolver _messagingTopicResolver;
        private readonly MessagingSubscriberOptions _subscriberOptions;

        public MessagingEventStoreSubscriber(IMessageBusSubscriber<object> messageBusSubscriber, MessagingTopicResolver messagingTopicResolver, MessagingSubscriberOptions subscriberOptions)
        {
            _messageBusSubscriber = messageBusSubscriber;
            _messagingTopicResolver = messagingTopicResolver;
            _subscriberOptions = subscriberOptions;
        }


        public Task SubscribeToAllAsync(Func<object, Task> handler, CancellationToken cancellationToken = default)
        {
            return _messageBusSubscriber.SubscribeAsync(async envelope =>
            {
                using (CorrelationManager.NewCorrelationId(envelope.GetCorrelationId()))
                {
                    await handler(envelope.Payload);
                }
            }, cancellationToken, _messagingTopicResolver.ResolveTopicName(), _subscriberOptions);
        }
    }
}
