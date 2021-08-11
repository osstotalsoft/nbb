using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Rx.Tests
{
    public class RangeMockSubscriber : IMessageBusSubscriber
    {
        private readonly int _start;
        private readonly int _count;
        private readonly List<Func<MessagingEnvelope, Task>> _handlers = new();

        public RangeMockSubscriber(int start, int count)
        {
            _start = start;
            _count = count;
        }

        public Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
        {

            _handlers.Add(envelope =>
                handler(new MessagingEnvelope<TMessage>(envelope.Headers, (TMessage)envelope.Payload)));

            return Task.FromResult<IDisposable>(null);
        }

        public void Start()
        {
            var nrs = Enumerable.Range(_start, _count);
            foreach (var nr in nrs)
            {
                foreach (var h in _handlers)
                {
                    h(new MessagingEnvelope(new Dictionary<string,string>(), nr));
                }
            }
        }
    }
}