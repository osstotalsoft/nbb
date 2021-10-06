// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using Proto.V1;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Rusi
{
    internal class RusiMessagingTransport : IMessagingTransport, ITransportMonitor
    {
        private readonly Proto.V1.Rusi.RusiClient _client;
        private readonly IOptions<RusiOptions> _options;
        private readonly ILogger<RusiMessagingTransport> _logger;

        public RusiMessagingTransport(Proto.V1.Rusi.RusiClient client, IOptions<RusiOptions> options,
            ILogger<RusiMessagingTransport> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        public event TransportErrorHandler OnError;

        public async Task PublishAsync(string topic, TransportSendContext sendContext,
            CancellationToken cancellationToken = default)
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

        public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
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

            var subRequest = new SubscriptionRequest()
            {
                PubsubName = _options.Value.PubsubName,
                Topic = topic,
                Options = subscriptionOptions
            };

            var subscription = _client.Subscribe();
            await subscription.RequestStream.WriteAsync(new SubscribeRequest() { SubscriptionRequest = subRequest });
            var returnDisposable = new DisposableSubscriptionWrapper() { InternalSubscription = subscription };

            //if awaited, blocks
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in returnDisposable.InternalSubscription.ResponseStream.ReadAllAsync())
                    {
                        var receiveContext = new TransportReceiveContext(
                            new TransportReceivedData.PayloadBytesAndHeaders(msg.Data.ToByteArray(), msg.Metadata));

                        //handle request
                        //this should never throw, application errors should be caught upstream
                        await handler(receiveContext);

                        //send ack
                        //async call, does not wait for pubsub ack
                        var ack = new AckRequest() { MessageId = msg.Id };
                        await returnDisposable.InternalSubscription.RequestStream.WriteAsync(new SubscribeRequest()
                        {
                            AckRequest = ack
                        });
                    }
                }
                catch (RpcException ex)
                {
                    _logger.LogError(ex, "Rusi transport unrecoverable exception");

                    OnError?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Rusi transport exception");
                }

            }, cancellationToken);

            return Task.FromResult<IDisposable>(subscription);
        }

        private class DisposableSubscriptionWrapper : IDisposable
        {
            internal AsyncDuplexStreamingCall<SubscribeRequest, ReceivedMessage> InternalSubscription { set; get; }

            public void Dispose()
            {
                InternalSubscription?.Dispose();
            }
        }
    }
}
