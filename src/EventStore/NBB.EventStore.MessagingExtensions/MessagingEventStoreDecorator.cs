using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Messaging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.MessagingExtensions
{
    public class MessagingEventStoreDecorator : IEventStore
    {
        private readonly IEventStore _innerEventStore;
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly MessagingTopicResolver _messagingTopicResolver;

        public MessagingEventStoreDecorator(IEventStore innerEventStore, IMessageBusPublisher messageBusPublisher, MessagingTopicResolver messagingTopicResolver)
        {
            _innerEventStore = innerEventStore;
            _messageBusPublisher = messageBusPublisher;
            _messagingTopicResolver = messagingTopicResolver;
        }

        public async Task AppendEventsToStreamAsync(string stream, IEnumerable<object> events, int? expectedVersion,
            CancellationToken cancellationToken = default)
        {
            var eventList = events.ToList();
            await _innerEventStore.AppendEventsToStreamAsync(stream, eventList, expectedVersion, cancellationToken);

            foreach (var @event in eventList)
            {
                await _messageBusPublisher.PublishAsync(@event, cancellationToken, null, _messagingTopicResolver.ResolveTopicName());
            }
        }

        public Task<List<object>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            return _innerEventStore.GetEventsFromStreamAsync(stream, startFromVersion, cancellationToken);
        }
    }
}
