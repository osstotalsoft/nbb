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
        private readonly ConcurrentDictionary<string, ValueTuple<IMessagingTopicSubscriber, List<InvocationHandler>>> _handlers;
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
            _handlers = new ConcurrentDictionary<string, (IMessagingTopicSubscriber, List<InvocationHandler>)>();
        }

        public async Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token, string topicName = null,
            MessagingSubscriberOptions options = null)
        {
            var _topicName = _topicRegistry.GetTopicForName(topicName) ?? _topicRegistry.GetTopicForMessageType(typeof(TMessage));
            var invocationHandler = new InvocationHandler(envelope => handler(envelope as MessagingEnvelope<TMessage>));
            Task subscribeAsync = null;

            var (_, handlersList) = _handlers.AddOrUpdate(_topicName,
                _ =>
                {
                    var topicSubscriber = ActivatorUtilities.CreateInstance<IMessagingTopicSubscriber>(_serviceProvider);
                    subscribeAsync = topicSubscriber.SubscribeAsync(_topicName, HandleMessage<TMessage>(options, _topicName), token, options);
                    return (topicSubscriber, new List<InvocationHandler> {invocationHandler});
                },
                (topic, listTuple) =>
                {
                    lock (listTuple.Item2)
                    {
                        //invocationHandler.MessagingTopicSubscriber = list.First().MessagingTopicSubscriber;
                        listTuple.Item2.Add(invocationHandler);
                    }

                    return listTuple;
                });

            if (subscribeAsync != null)
                await subscribeAsync;

            return new Subscription(invocationHandler, handlersList);
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
                    var (_, handlersList) = _handlers[topicName];
                    foreach (var handler in handlersList)
                    {
                        await handler.Handler(deserializedMessage);
                    }
                }
            };
        }

        public async Task UnSubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token)
        {
            var _topicName = _topicRegistry.GetTopicForMessageType(typeof(TMessage));
            var (messagingTopicSubscriber, handlersList) = _handlers[_topicName];

            handlersList.RemoveAll(x => x.Handler == handler);
            if (handlersList.Count == 0)
            {
                await messagingTopicSubscriber.UnSubscribeAsync(token);
                _handlers.TryRemove(_topicName, out _);
            }
        }
    }

    internal struct InvocationHandler
    {
        public Func<MessagingEnvelope, Task> Handler { get; set; }

        public InvocationHandler(Func<MessagingEnvelope, Task> handler)
        {
            Handler = handler;
        }
    }

    internal class Subscription : IDisposable
    {
        private readonly InvocationHandler _invocationHandler;
        private readonly List<InvocationHandler> _list;

        public Subscription(InvocationHandler invocationHandler, List<InvocationHandler> list)
        {
            this._invocationHandler = invocationHandler;
            this._list = list;
        }

        public void Dispose()
        {
            this._list.Remove(this._invocationHandler);
        }
    }
}