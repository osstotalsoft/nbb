// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream;

public class JetStreamMessagingTransport(IOptions<JetStreamOptions> natsOptions, INatsConnectionPool connectionPool, ILogger<JetStreamMessagingTransport> logger) : IMessagingTransport, ITransportMonitor
{
    public event TransportErrorHandler OnError;

    public async Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
    {
        var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();
        await connectionPool.GetConnection().PublishAsync(topic, envelopeData, cancellationToken: cancellationToken);
        //PubAckResponse ack = await natsJSContext.PublishAsync(topic, envelopeData, cancellationToken: cancellationToken);
        //ack.EnsureSuccess();
    }

    public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
        SubscriptionTransportOptions options = null, CancellationToken token = default)
    {
        var stream = string.Empty;
        var natsJSContext = connectionPool.GetConnection().CreateJetStreamContext();
        await foreach (var item in natsJSContext.ListStreamNamesAsync(topic, token)) { stream = item; }

        var subscriberOptions = options ?? SubscriptionTransportOptions.Default;

        var cc = new ConsumerConfig();
        var consumerName = (natsOptions.Value.ClientId + "__" + topic).Replace(".", "_");
        // Consumers without a DurableName are ephemeral: the server removes them once nobody pulls from them.
        // They must still be named, otherwise the create subject is built from the filter subject and the server rejects it.
        if (subscriberOptions.UseGroup)
        {
            // Subscribers with the same client id share one consumer, so each message goes to only one of them.
            cc.Name = consumerName;
            if (subscriberOptions.IsDurable)
                cc.DurableName = consumerName;
        }
        else
        {
            // Every subscriber gets its own ephemeral consumer, so each one receives every message (fan-out).
            // A durable consumer would be shared by every subscriber with the same client id, so durability is dropped.
            if (subscriberOptions.IsDurable)
                logger.LogWarning(
                    "IsDurable = true is ignored for the subscription to {Topic} because UseGroup = false: each subscriber gets its own ephemeral consumer and receives every message, but messages published while it is not running are not delivered to it",
                    topic);

            cc.Name = consumerName + "__" + Guid.NewGuid().ToString("N");
        }

        if (subscriberOptions.DeliverNewMessagesOnly)
            cc.DeliverPolicy = ConsumerConfigDeliverPolicy.New;

        cc.AckWait = TimeSpan.FromMilliseconds(subscriberOptions.AckWait ?? natsOptions.Value.AckWait ?? 50000);
        cc.FilterSubject = topic;
        //cc.InactiveThreshold = TimeSpan.FromMinutes(5s);
        cc.AckPolicy = ConsumerConfigAckPolicy.Explicit;

        var consumer = await natsJSContext.CreateOrUpdateConsumerAsync(stream, cc, token);
        var cts = new CancellationTokenSource();

        async Task HandleMessage(INatsJSMsg<byte[]> msg)
        {
            var receiveContext = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(msg.Data));
            await handler(receiveContext);
            await msg.AckAsync(cancellationToken: cts.Token);
        }

        var t = Task.Run(async () =>
        {
            var consumeOptions = new NatsJSConsumeOpts { MaxMsgs = subscriberOptions.MaxConcurrentMessages };

            try
            {
                await Parallel.ForEachAsync(
                    consumer.ConsumeAsync<byte[]>(opts: consumeOptions, cancellationToken: cts.Token),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = subscriberOptions.MaxConcurrentMessages,
                        CancellationToken = cts.Token,
                    },
                    async (msg, _) => await HandleMessage(msg));
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _ = Task.Run(() => OnError?.Invoke(e));
            }
        });

        //should be asyncDisposable
        return new SubscriptionDisposable(() =>
        {
            cts.Cancel();
            t.Wait();
        });
    }
}

internal record SubscriptionDisposable(Action dispose) : IDisposable
{
    public void Dispose()
    {
        dispose();
    }
}
