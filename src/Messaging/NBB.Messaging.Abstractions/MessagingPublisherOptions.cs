// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Messaging.Abstractions
{
    public record MessagingPublisherOptions
    {
        public Action<MessagingEnvelope> EnvelopeCustomizer { get; init; }

        public string TopicName { get; init; }

        public static MessagingPublisherOptions Default = new ();
   }
}
