using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Rx
{
    public interface IMessagingObservable<TMessage> : IObservable<MessagingEnvelope<TMessage>>, IDisposable
    {
      
    }
}
