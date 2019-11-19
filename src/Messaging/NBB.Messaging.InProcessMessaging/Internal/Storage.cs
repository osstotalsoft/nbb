using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public class Storage : IStorage
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        private readonly HashSet<string> _subscriptions = new HashSet<string>();
        private readonly ConcurrentDictionary<string, AutoResetEvent> _brokersAutoReset = new ConcurrentDictionary<string, AutoResetEvent>();

        public void Enqueue(string msg, string topic)
        {
            var q = _queues.GetOrAdd(topic, new ConcurrentQueue<string>());
            q.Enqueue(msg);

            lock (_subscriptions)
            {
                if (_subscriptions.Contains(topic))
                {
                    AwakeBroker(topic);
                }

            }
        }

        public async Task AddSubscription(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default) 
        {
            lock (_subscriptions)
            {
                if (_subscriptions.Contains(topic))
                    throw new Exception("Already subscribed to topic " + topic);

                _subscriptions.Add(topic);
            }

            await Task.Yield();
            var startBrokerTask = Task.Run(async () => { await StartBroker(topic, handler, cancellationToken); }, cancellationToken);
        }

        private async Task StartBroker(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default)
        {
            var ev = _brokersAutoReset.GetOrAdd(topic, new AutoResetEvent(false));

            var q = _queues.GetOrAdd(topic, new ConcurrentQueue<string>());
            while (!cancellationToken.IsCancellationRequested)
            {
                if (q.IsEmpty)
                    ev.WaitOne();

                q.TryDequeue(out string msg);
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
