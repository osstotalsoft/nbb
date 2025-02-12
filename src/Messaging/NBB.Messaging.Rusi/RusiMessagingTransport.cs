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
#if NETCOREAPP3_0_OR_GREATER
using System.Threading.Channels;
#endif
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

            if (string.IsNullOrEmpty(_options.Value.PubsubName))
            {
                logger.LogWarning("no PubsubName provided, using default rusi pubsub name");
            }
        }

        public event TransportErrorHandler OnError;

        public async Task PublishAsync(string topic, TransportSendContext sendContext,
            CancellationToken cancellationToken = default)
        {
            var (payload, extraHeaders) = sendContext.PayloadBytesAccessor.Invoke();
            var headers = sendContext.HeadersAccessor.Invoke()
                .Concat(extraHeaders)
                .Where(h => h.Value != null)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Last().Value);

            var request = new PublishRequest()
            {
                PubsubName = _options.Value.PubsubName ?? string.Empty,
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
#if NETCOREAPP3_0_OR_GREATER
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
                PubsubName = _options.Value.PubsubName ?? string.Empty,
                Topic = topic,
                Options = subscriptionOptions
            };

            var ackChannel = Channel.CreateUnbounded<AckRequest>(new() { SingleReader = true, SingleWriter = false });
            var subscription = _client.Subscribe(cancellationToken: cancellationToken);
            await subscription.RequestStream.WriteAsync(new() { SubscriptionRequest = subRequest });

            _ = Task.Run(async () =>
            {
                await foreach (var ack in ackChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    await subscription?.RequestStream.WriteAsync(new() { AckRequest = ack });
                }
            }, cancellationToken);

            //if awaited, blocks
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in subscription.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        var receiveContext = new TransportReceiveContext(
                            new TransportReceivedData.PayloadBytesAndHeaders(msg.Data.ToByteArray(), msg.Metadata));

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await handler(receiveContext);
                                await ackChannel.Writer.WriteAsync(new() { MessageId = msg.Id });
                            }
                            catch (Exception ex)
                            {
                                await ackChannel.Writer.WriteAsync(new() { MessageId = msg.Id, Error = ex.Message });
                                throw;
                            }
                        });
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    _logger.LogDebug("Rusi message stream cancelled");
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Rusi transport unrecoverable exception");

                    OnError?.Invoke(ex);
                }
            }, cancellationToken);

            return subscription;
#else
            throw new NotSupportedException("Rusi subscriptions are only supported on netcoreapp3.0 or greater");
#endif
        }
    }
}
