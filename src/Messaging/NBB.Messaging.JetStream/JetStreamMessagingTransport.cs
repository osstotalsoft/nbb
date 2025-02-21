// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NBB.Messaging.Abstractions;
using NBB.Messaging.JetStream.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream;

public class JetStreamMessagingTransport : IMessagingTransport, ITransportMonitor
{
    private readonly IOptions<JetStreamOptions> _natsOptions;
    private readonly JetStreamConnectionProvider _natsConnectionManager;

    public JetStreamMessagingTransport(IOptions<JetStreamOptions> natsOptions, JetStreamConnectionProvider natsConnectionManager)
    {
        _natsOptions = natsOptions;
        _natsConnectionManager = natsConnectionManager;
    }

    public event TransportErrorHandler OnError;

    public async Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
    {
        var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();
        await _natsConnectionManager.GetConnection().PublishAsync(topic, envelopeData, cancellationToken: cancellationToken);
    }

    public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
        SubscriptionTransportOptions options = null, CancellationToken token = default)
    {
        var stream = string.Empty;
        var js = new NatsJSContext(_natsConnectionManager.GetConnection());
        await foreach (var item in js.ListStreamNamesAsync(topic, token)) { stream = item; }

        var subscriberOptions = options ?? SubscriptionTransportOptions.Default;

        var cc = new ConsumerConfig();
        if (subscriberOptions.IsDurable)
        {
            var clientId = (_natsOptions.Value.ClientId + "__" + topic).Replace(".", "_");
            cc.Name = clientId;
            cc.DurableName = clientId;
        }

        if (subscriberOptions.DeliverNewMessagesOnly)
            cc.DeliverPolicy = ConsumerConfigDeliverPolicy.New;

        cc.AckWait = TimeSpan.FromMilliseconds(subscriberOptions.AckWait ?? _natsOptions.Value.AckWait ?? 50000);
        cc.FilterSubject = topic;
        //cc.InactiveThreshold = TimeSpan.FromMinutes(5s);
        cc.AckPolicy = ConsumerConfigAckPolicy.Explicit;

        var consumeOptions = new NatsJSConsumeOpts
        {
            MaxMsgs = subscriberOptions.MaxConcurrentMessages,
        };
        var consumer = await js.CreateOrUpdateConsumerAsync(stream, cc, token);

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
