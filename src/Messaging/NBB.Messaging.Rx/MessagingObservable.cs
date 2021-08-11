using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Rx
{
    public class MessagingObservable<TMessage> : IMessagingObservable<TMessage>
    {
        private readonly IDisposable _subscription;
        private readonly ConcurrentDictionary<int, ObserverSubscription> _subscriptions = new();

        public MessagingObservable(IMessageBusSubscriber messageBusSubscriber)
        {
            //TODO improve async
            _subscription = messageBusSubscriber.SubscribeAsync<TMessage>(InvokeObservers).GetAwaiter().GetResult();
        }

        private Task InvokeObservers(MessagingEnvelope<TMessage> data)
        {

            foreach (var sub in _subscriptions.Values)
            {
                try
                {
                    sub.Observer.OnNext(data);
                }
                catch
                {
                    _subscriptions.TryRemove(sub.Id, out _);

                    try
                    {
                        sub.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return Task.CompletedTask;
        }

        public IDisposable Subscribe(IObserver<MessagingEnvelope<TMessage>> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            var sub = new ObserverSubscription(observer, DisposeSubscription);

            if (!_subscriptions.TryAdd(sub.Id, sub))
                throw new ArgumentException("Could not subscribe observer. Ensure it has not already been subscribed.",
                    nameof(observer));

            return sub;


        }

        private void DisposeSubscription(ObserverSubscription sub)
        {
            if (!_subscriptions.TryRemove(sub.Id, out _))
                return;

            try
            {
                sub.Observer.OnCompleted();
            }
            catch
            {
                // ignored
            }
        }


        private sealed class ObserverSubscription : IDisposable
        {
            private readonly Action<ObserverSubscription> _disposer;
            public readonly int Id;
            public readonly IObserver<MessagingEnvelope<TMessage>> Observer;

            public ObserverSubscription(IObserver<MessagingEnvelope<TMessage>> observer,
                Action<ObserverSubscription> disposer)
            {
                Id = observer.GetHashCode();
                Observer = observer;
                _disposer = disposer;
            }

            public void Dispose() => _disposer(this);
        }

        public void Dispose()
        {
            _subscription?.Dispose();

            var exceptions = new List<Exception>();

            foreach (var obSub in _subscriptions)
            {
                try
                {
                    _subscriptions.TryRemove(obSub.Key, out _);
                    obSub.Value.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new Exception(
                    "One or more exception(s) occurred while disposing the NATS Observable's' subscriptions. See inner exception (AggregateException) for more details.",
                    new AggregateException(exceptions));
        }
    }
}