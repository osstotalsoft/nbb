// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using Proto.V1;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Rusi
{
    public class RusiMessageBusSubscriber : IMessageBusSubscriber
    {
        private readonly Proto.V1.Rusi.RusiClient _client;
        private readonly IOptions<RusiOptions> _options;
        private readonly ITopicRegistry _topicRegistry;
        private readonly IMessageSerDes _messageSerDes;
        private readonly ILogger<RusiMessageBusSubscriber> _logger;

        public RusiMessageBusSubscriber(Proto.V1.Rusi.RusiClient client, IOptions<RusiOptions> options,
            ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes,
            ILogger<RusiMessageBusSubscriber> logger)
        {
            _client = client;
            _options = options;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _logger = logger;
        }

        public Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
        {

            var topicName = _topicRegistry.GetTopicForName(options?.TopicName) ??
                            _topicRegistry.GetTopicForMessageType(typeof(TMessage));

            async Task MsgHandler(ReceivedMessage messageData)
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topicName);

                try
                {
                    var payload = _messageSerDes.DeserializePayload<TMessage>(messageData.Data.ToByteArray(), messageData.Metadata);
                    var messageEnvelope = new MessagingEnvelope<TMessage>(messageData.Metadata, payload);

                    await handler(messageEnvelope);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Messaging consumer encountered an error when handling a message from subject {Subject}.\n {Error}",
                        topicName, ex);

                    //TODO: push to DLQ
                }
            }

            var subscription = _client.Subscribe(new SubscribeRequest()
            {
                PubsubName = _options.Value.PubsubName,
                Topic = topicName
            });


            //if awaited, blocks
            _ = Task.Run(async () =>
                {
                    await foreach (var msg in subscription.ResponseStream.ReadAllAsync())
                    {
                        await MsgHandler(msg);
                    }
                });

            return Task.FromResult<IDisposable>(subscription);
        }
    }
}
