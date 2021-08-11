using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Rx
{
    public static class IMessageBusSubscriberExtensions
    {

        public static IMessagingObservable<TMessage> Observe<TMessage>(this IMessageBusSubscriber subscriber) =>
            new MessagingObservable<TMessage>(subscriber);

    }
}