using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public class Storage : IStorage
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> _queues = new();
        private readonly HashSet<string> _subscriptions = new();
        private readonly ConcurrentDictionary<string, AutoResetEvent> _brokersAutoReset = new();

        public void Enqueue(byte[] msg, string topic)
        {
            var q = _queues.GetOrAdd(topic, new ConcurrentQueue<byte[]>());
            q.Enqueue(msg);

            lock (_subscriptions)
            {
                if (_subscriptions.Contains(topic))
                {
                    AwakeBroker(topic);
                }
            }
        }

        public async Task AddSubscription(string topic, Func<byte[], Task> handler,
            CancellationToken cancellationToken = default)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.Contains(topic))
                    throw new Exception("Already subscribed to topic " + topic);

                _subscriptions.Add(topic);
            }

            await Task.Yield();
            var _ = Task.Run(async () => { await StartBroker(topic, handler, cancellationToken); }, cancellationToken);
        }

        private async Task StartBroker(string topic, Func<byte[], Task> handler,
            CancellationToken cancellationToken = default)
        {
            var ev = _brokersAutoReset.GetOrAdd(topic, new AutoResetEvent(false));

            var q = _queues.GetOrAdd(topic, new ConcurrentQueue<byte[]>());
            while (!cancellationToken.IsCancellationRequested)
            {
                if (q.IsEmpty)
                    ev.WaitOne();

                q.TryDequeue(out var msg);
                await handler(msg);
            }
        }

        private void AwakeBroker(string topic)
        {
            var ev = _brokersAutoReset[topic];
            ev?.Set();
        }
    }
}