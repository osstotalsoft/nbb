// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using Polly;
using Proto.V1;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Rusi
{
    internal class RusiMessagingTransport : IMessagingTransport
    {
        private readonly Proto.V1.Rusi.RusiClient _client;
        private readonly IOptions<RusiOptions> _options;

        public RusiMessagingTransport(Proto.V1.Rusi.RusiClient client, IOptions<RusiOptions> options)
        {
            _client = client;
            _options = options;
        }

        public async Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            var (payload, extraHeaders) = sendContext.PayloadBytesAccessor.Invoke();
            var headers = sendContext.HeadersAccessor.Invoke()
                        .Concat(extraHeaders)
                        .GroupBy(x => x.Key)
                        .ToDictionary(x => x.Key, x => x.Last().Value);

            var request = new PublishRequest()
            {
                PubsubName = _options.Value.PubsubName,
                Topic = topic,
                Data = ByteString.CopyFrom(payload),
                DataContentType = "application/json;charset=utf-8",
                Metadata = { headers }
            };

            await _client.PublishAsync(request);
        }

        public Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
            TransportReceiveContextFactory receiveContextFactory,
            SubscriptionTransportOptions options = null, CancellationToken cancellationToken = default)
        {
            var transport = options ?? SubscriptionTransportOptions.Default;
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
                Topic = topic,
                Options = subscriptionOptions
            };

            var subscription = _client.Subscribe(subRequest);
            var returnDisposable = new DisposableSubscriptionWrapper() { InternalSubscription = subscription };

            var policy = Policy
                .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
                .WaitAndRetryAsync(20, i => TimeSpan.FromSeconds(i * 2),
                    (exception, span) => {
                        returnDisposable.InternalSubscription?.Dispose();
                        returnDisposable.InternalSubscription = null;
                    })
                ;

            //if awaited, blocks
            _ = policy.ExecuteAsync(async (_) =>
                {
                    returnDisposable.InternalSubscription ??= _client.Subscribe(subRequest);
                    await foreach (var msg in returnDisposable.InternalSubscription.ResponseStream.ReadAllAsync())
                    {
                        var receiveContext = receiveContextFactory.FromPayloadBytesAndHeaders(msg.Data.ToByteArray(), msg.Metadata);
                        await handler(receiveContext);
                    }

                }, cancellationToken);

            return Task.FromResult<IDisposable>(subscription);
        }

        private class DisposableSubscriptionWrapper : IDisposable
        {
            internal AsyncServerStreamingCall<ReceivedMessage> InternalSubscription { set; get; }

            public void Dispose()
            {
                InternalSubscription?.Dispose();
            }
        }
    }
}
