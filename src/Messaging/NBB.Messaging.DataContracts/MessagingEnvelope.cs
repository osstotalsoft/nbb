using System;
using System.Collections.Generic;

namespace NBB.Messaging.DataContracts
{
    public class MessagingEnvelope
    {
        public Dictionary<string, string> Headers { get; private set; }

        public object Payload { get; private set; }

        public MessagingEnvelope(Dictionary<string, string> headers, object payload)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }
    }

    public class MessagingEnvelope<TMessage> : MessagingEnvelope
    {
        public MessagingEnvelope(Dictionary<string, string> headers, TMessage payload)
            : base(headers, payload)
        {
        }

        public new TMessage Payload => (TMessage)base.Payload;
    }
}
