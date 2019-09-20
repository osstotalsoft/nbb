using Microsoft.Extensions.Logging;
using NBB.Messaging.DataContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusSubscriber : IMessageBusSubscriber
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITopicRegistry _topicRegistry;
        private readonly ConcurrentDictionary<string, List<InvocationHandler>> _handlers = new ConcurrentDictionary<string, List<InvocationHandler>>();
        private readonly IMessageSerDes _messageSerDes;
        private readonly ILogger<MessageBusSubscriber> _logger;

        public MessageBusSubscriber(IServiceProvider serviceProvider, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes,
            ILogger<MessageBusSubscriber> logger)
        {
            _serviceProvider = serviceProvider;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _logger = logger;
        }

        public Task SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token, string topicName = null,
            MessagingSubscriberOptions options = null)
        {
            var _topicName = _topicRegistry.GetTopicForName(topicName) ?? _topicRegistry.GetTopicForMessageType(typeof(TMessage));

            var invocationHandler = new InvocationHandler(options, envelope => handler(envelope as MessagingEnvelope<TMessage>));

            _handlers.AddOrUpdate(_topicName,
                _ =>
                {
                    var topicSubscriber = ActivatorUtilities.CreateInstance<IMessagingTopicSubscriber>(_serviceProvider);
                    topicSubscriber.SubscribeAsync(_topicName, HandleMessage<TMessage>(options, _topicName), token, options);
                    invocationHandler.MessagingTopicSubscriber = topicSubscriber;
                    return new List<InvocationHandler> {invocationHandler};
                },
                (topic, list) =>
                {
                    lock (list)
                    {
                        //invocationHandler.MessagingTopicSubscriber = list.First().MessagingTopicSubscriber;
                        list.Add(invocationHandler);
                    }

                    return list;
                });

            return Task.CompletedTask;

        }

        Func<string, Task> HandleMessage<TMessage>(MessagingSubscriberOptions options, string topicName)
        {
            return async msg =>
            {
                MessagingEnvelope<TMessage> deserializedMessage = null;
                try
                {
                    deserializedMessage = _messageSerDes.DeserializeMessageEnvelope<TMessage>(msg, options?.SerDes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "MessageBusSubscriber encountered an error when deserializing a message from topic {TopicName}.\n {Error}",
                        topicName, ex);
                    //TODO: push to DLQ
                }


                if (deserializedMessage != null)
                {
                    foreach (var handler in _handlers[topicName].ToList())
                    {
                        await handler.Handler(deserializedMessage);
                    }
                }
            };
        }

        public async Task UnSubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token)
        {
            var _topicName = _topicRegistry.GetTopicForMessageType(typeof(TMessage));
            var messagingTopicSubscriber = _handlers[_topicName].Where(x => x.MessagingTopicSubscriber != null).Select(x => x.MessagingTopicSubscriber).First();

            _handlers[_topicName].RemoveAll(x => x.Handler == handler);
            if (_handlers[_topicName].Count == 0)
                await messagingTopicSubscriber.UnSubscribeAsync(token);
        }
    }

    public class InvocationHandler
    {
        public IMessagingTopicSubscriber MessagingTopicSubscriber { get; set; }
        public MessagingSubscriberOptions Options { get; set; }
        public Func<MessagingEnvelope, Task> Handler { get; set; }

        public InvocationHandler(MessagingSubscriberOptions options, Func<MessagingEnvelope, Task> handler)
        {
            Options = options;
            Handler = handler;
        }
    }
}