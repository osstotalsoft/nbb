// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace NBB.Messaging.Abstractions
{
    public record MessagingEnvelope
    {
        public IDictionary<string, string> Headers { get; }

        public object Payload { get; }

        public MessagingEnvelope(IDictionary<string, string> headers, object payload)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }
    }

    public record MessagingEnvelope<TMessage> : MessagingEnvelope
    {
        public MessagingEnvelope(IDictionary<string, string> headers, TMessage payload)
            : base(headers, payload)
        {
        }

        public new TMessage Payload => (TMessage) base.Payload;
    }
}
