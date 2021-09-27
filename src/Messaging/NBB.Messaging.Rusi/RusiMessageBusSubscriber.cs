// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using Proto.V1;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Polly;

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


            var transport = options?.Transport ?? SubscriptionTransportOptions.Default;
            var subscriptionOptions = new SubscriptionOptions()
            {
                DeliverNewMessagesOnly = transport.DeliverNewMessagesOnly,
                MaxConcurrentMessages = transport.MaxConcurrentMessages,
                QGroup = transport.UseGroup,
                Durable = transport.IsDurable
            };

            if (transport.AckWait.HasValue)
                subscriptionOptions.AckWaitTime =
                    Duration.FromTimeSpan(TimeSpan.FromMilliseconds(transport.AckWait.Value));

            var subRequest = new SubscribeRequest()
            {
                PubsubName = _options.Value.PubsubName,
                Topic = topicName,
                Options = subscriptionOptions
            };

            var subscription = _client.Subscribe(subRequest);
            var returnDisposable = new DisposableSubscriptionWrapper() { InternalSubscription = subscription };

            var policy = Policy
                .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
                .WaitAndRetryAsync(20, i => TimeSpan.FromSeconds(Math.Pow(i, 2)),
                    (exception, span) => { returnDisposable.InternalSubscription = null; });

            var msgHandler = GetHandler(handler, topicName, options);

            //if awaited, blocks
            _ = Task.Run(async () =>
            {
                await policy.ExecuteAsync(async (_) =>
                {
                    returnDisposable.InternalSubscription ??= _client.Subscribe(subRequest);
                    await foreach (var msg in returnDisposable.InternalSubscription.ResponseStream.ReadAllAsync())
                    {
                        await msgHandler(msg);
                    }

                }, cancellationToken);

                // TODO send to DLQ
            });

            return Task.FromResult<IDisposable>(returnDisposable);
        }

        private class DisposableSubscriptionWrapper : IDisposable
        {
            internal AsyncServerStreamingCall<Proto.V1.ReceivedMessage> InternalSubscription { set; get; }

            public void Dispose()
            {
                InternalSubscription?.Dispose();
            }
        }

        public Func<ReceivedMessage, Task> GetHandler<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            string topicName, MessagingSubscriberOptions options)
        {
            return async messageData =>
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topicName);

                try
                {
                    var payload =
                        _messageSerDes.DeserializePayload<TMessage>(messageData.Data.ToByteArray(),
                            messageData.Metadata, options?.SerDes);
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
            };
        }
    }
}
