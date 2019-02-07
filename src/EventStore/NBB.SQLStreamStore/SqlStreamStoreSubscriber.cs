using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.SQLStreamStore.Internal;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace NBB.SQLStreamStore
{
    public class SqlStreamStoreSubscriber : IEventStoreSubscriber
    {
        private readonly IStreamStore _store;
        private readonly ISerDes _serDes;
        private readonly ILogger<SqlStreamStoreSubscriber> _logger;

        public SqlStreamStoreSubscriber(IStreamStore store, ISerDes serDes, ILogger<SqlStreamStoreSubscriber> logger)
        {
            _store = store;
            _serDes = serDes;
            _logger = logger;
        }


        public Task SubscribeToAllAsync(Func<IEvent, Task> handler, CancellationToken token)
        {
            var are = new AutoResetEvent(false);
            var sub = _store.SubscribeToAll(null, (s, m, t) => MessageReceived(s, m, handler, are, t), null, null, true, "x");
            are.WaitOne();

            return Task.CompletedTask;
        }


        private async Task MessageReceived(IAllStreamSubscription subscription, StreamMessage streamMessage, Func<IEvent, Task> handler, AutoResetEvent are, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                are.Set();

            var metadata = _serDes.Deserialize<EventMetadata>(streamMessage.JsonMetadata);
            var eventType = metadata.GetEventType();
            _logger.LogDebug("SqlStreamStoreSubscriber subscriber received message of type {EventType}", eventType.FullName);
            var eventData = await streamMessage.GetJsonData(token);
            var @event = _serDes.Deserialize(eventData, eventType) as IEvent;

            await handler(@event);
        }
    }
}
