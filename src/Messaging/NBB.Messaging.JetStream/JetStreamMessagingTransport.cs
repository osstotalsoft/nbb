// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream;

public class JetStreamMessagingTransport(IOptions<JetStreamOptions> natsOptions, INatsJSContext natsJSContext) : IMessagingTransport, ITransportMonitor
{
    public event TransportErrorHandler OnError;

    public async Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
    {
        var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();
        await natsJSContext.Connection.PublishAsync(topic, envelopeData, cancellationToken: cancellationToken);
        //PubAckResponse ack = await natsJSContext.PublishAsync(topic, envelopeData, cancellationToken: cancellationToken);
        //ack.EnsureSuccess();
    }

    public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
        SubscriptionTransportOptions options = null, CancellationToken token = default)
    {
        var stream = string.Empty;
        await foreach (var item in natsJSContext.ListStreamNamesAsync(topic, token)) { stream = item; }

        var subscriberOptions = options ?? SubscriptionTransportOptions.Default;

        var cc = new ConsumerConfig();
        if (subscriberOptions.IsDurable)
        {
            var clientId = (natsOptions.Value.ClientId + "__" + topic).Replace(".", "_");
            cc.Name = clientId;
            cc.DurableName = clientId;
        }

        if (subscriberOptions.DeliverNewMessagesOnly)
            cc.DeliverPolicy = ConsumerConfigDeliverPolicy.New;

        cc.AckWait = TimeSpan.FromMilliseconds(subscriberOptions.AckWait ?? natsOptions.Value.AckWait ?? 50000);
        cc.FilterSubject = topic;
        //cc.InactiveThreshold = TimeSpan.FromMinutes(5s);
        cc.AckPolicy = ConsumerConfigAckPolicy.Explicit;

        var consumeOptions = new NatsJSConsumeOpts
        {
            MaxMsgs = subscriberOptions.MaxConcurrentMessages,
        };
        var consumer = await natsJSContext.CreateOrUpdateConsumerAsync(stream, cc, token);

        var cts = new CancellationTokenSource();
        var t = Task.Run(async () =>
        {
            try
            {
                //await consumer.RefreshAsync(token);
                await foreach (var msg in consumer.ConsumeAsync<byte[]>(opts: consumeOptions, cancellationToken: cts.Token))
                {
                    var receiveContext = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(msg.Data));
                    await handler(receiveContext);
                    await msg.AckAsync(cancellationToken: cts.Token);
                }
            }
            //catch (NatsJSProtocolException e)
            //catch (NatsJSException e)
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
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
