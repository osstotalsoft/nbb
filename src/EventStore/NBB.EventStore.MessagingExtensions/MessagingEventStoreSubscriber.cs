using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Internal;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.EventStore.MessagingExtensions
{
    public class MessagingEventStoreSubscriber : IEventStoreSubscriber
    {
        private readonly IEventStoreSerDes _eventStoreSerDes;
        private readonly IMessageBusSubscriber _messageBusSubscriber;
        private readonly MessagingTopicResolver _messagingTopicResolver;
        private readonly MessagingSubscriberOptions _subscriberOptions;

        public MessagingEventStoreSubscriber(IEventStoreSerDes eventStoreSerDes, IMessageBusSubscriber messageBusSubscriber, MessagingTopicResolver messagingTopicResolver, MessagingSubscriberOptions subscriberOptions)
        {
            _eventStoreSerDes = eventStoreSerDes;
            _messageBusSubscriber = messageBusSubscriber;
            _messagingTopicResolver = messagingTopicResolver;
            _subscriberOptions = subscriberOptions;
        }


        public Task SubscribeToAllAsync(Func<IEvent, Task> handler, CancellationToken token)
        {
            return _messageBusSubscriber.SubscribeAsync<IEvent>(async envelope =>
            {
                using (CorrelationManager.NewCorrelationId(envelope.GetCorrelationId()))
                {
                    await handler(envelope.Payload);
                }
            }, token, _messagingTopicResolver.ResolveTopicName(), _subscriberOptions);
        }
    }
}
