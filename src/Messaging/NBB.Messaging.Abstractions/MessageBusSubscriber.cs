using Microsoft.Extensions.Logging;
using NBB.Messaging.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusSubscriber<TMessage> : IMessageBusSubscriber<TMessage> 
    {
        private readonly ITopicRegistry _topicRegistry;
        private readonly List<Func<MessagingEnvelope<TMessage>, Task>> _handlers = new List<Func<MessagingEnvelope<TMessage>, Task>>();
        private readonly IMessageSerDes _messageSerDes;
        private bool _subscribedToTopic;
        private readonly IMessagingTopicSubscriber _topicSubscriber;
        private readonly ILogger<MessageBusSubscriber<TMessage>> _logger;
        private string _topicName;
        private MessagingSubscriberOptions _subscriberOptions;

        public MessageBusSubscriber(IMessagingTopicSubscriber topicSubscriber, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes,
            ILogger<MessageBusSubscriber<TMessage>> logger)
        {
            _topicSubscriber = topicSubscriber;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _logger = logger;
        }

        public Task SubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token, string topicName = null, MessagingSubscriberOptions options = null)
        {
            _handlers.Add(handler);

            if (!_subscribedToTopic)
            {
                lock (this)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        _topicName = _topicRegistry.GetTopicForName(topicName) ??
                                     _topicRegistry.GetTopicForMessageType(typeof(TMessage));
                        _subscriberOptions = options;
                        _topicSubscriber.SubscribeAsync(_topicName, HandleMessage, token, options);
                    }
                }
            }

            return Task.CompletedTask;

        }

        async Task HandleMessage(string msg)
        {
            MessagingEnvelope<TMessage> deserializedMessage = null;
            try
            {
                 deserializedMessage = _messageSerDes.DeserializeMessageEnvelope<TMessage>(msg, _subscriberOptions?.SerDes);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "MessageBusSubscriber encountered an error when deserializing a message from topic {TopicName}.\n {Error}",
                    _topicName, ex);
                //TODO: push to DLQ
            }


            if (deserializedMessage != null)
            {
                foreach (var handler in _handlers.ToList())
                {
                    await handler(deserializedMessage);
                }
            }
        }

        public async Task UnSubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token)
        {
            _handlers.Remove(handler);
            if (_handlers.Count == 0)
                await _topicSubscriber.UnSubscribeAsync(token);
        }
    }
}
